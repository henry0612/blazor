using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Application.Services.Reports;
using UnifiedAccount.Batch.Framework;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Batch.Jobs.Report;

/// <summary>
/// サンプルレポート印刷
/// </summary>
/// <remarks>
/// 帳票出力のサンプルジョブです。
/// 実行例:
/// UnifiedAccount.Batch.exe --job SampleReport
/// </remarks>
public class SampleReport : JobBase
{
    public SampleReport(ITransactionCoordinator transactionCoordinator, ILoggerFactory loggerFactory, IJobRunner jobRunner)
        : base(transactionCoordinator, loggerFactory, jobRunner)
    {
    }

    public override string JobId => "SampleReport";
    public string TemplateId => "SAMPLE";
    public string DetailTemplateId => "SAMPLE-DETAIL";
    public override string Description => "サンプルレポート";
    /// <summary>
    /// ジョブ本体を実行する。
    /// </summary>
    /// <param name="context">ジョブコンテキスト</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>ジョブの実行結果</returns>
    /// <remarks>
    /// トランザクションは <see cref="JobBase"/> が管理するため、本メソッドでは開始も終了もしない。
    /// 単独起動時はこのジョブ、ワークフロー経由の場合はワークフローが境界となる。
    /// </remarks>
    protected override async Task<JobResult> ExecuteCoreAsync(JobContext context, CancellationToken ct)
    {
        var logger = Logger as ILogger<SampleReport> ?? context.Services.GetRequiredService<ILogger<SampleReport>>();
        var db = context.Services.GetRequiredService<AppDbContext>();
        var reportService = context.Services.GetRequiredService<IReportOutputService>();
        var filePathProvider = context.Services.GetRequiredService<IReportFilePathProvider>();
        var metrics = new JobMetrics();

        logger.LogInformation(
            "[{JobId}] {ExecutionId} SAMPLE-I001 {Description}開始 ProcessDate={ProcessDate:yyyy-MM-dd} TemplateId={TemplateId}",
            JobId,
            context.JobExecutionId,
            Description,
            context.ProcessDate,
            TemplateId);

        try
        {
            // データ取得
            var sourceQuery = db.CompanyMasterChangeLogs
                .AsNoTracking()
                .Where(e => e.JobExecutionId == context.JobExecutionId)
                .OrderBy(e => e.Id);

            var readCount = await sourceQuery.CountAsync(ct);
            metrics.IncrementRead(readCount);

            // 帳票出力設定
            var request = new ReportOutputRequest
            {
                ExecutionId = $"{context.JobExecutionId}-SAMPLE",
                JobExecutionId = context.JobExecutionId,
                ProcessDate = context.ProcessDate,
                TemplateId = TemplateId,
                // SplitKey = nameof(Company.CompanyCode),      会社コード別の印刷の場合
                DataRequest = new ReportDataRequest
                {
                    Mode = ReportDataMode.Streamed,
                    SourceData = sourceQuery.AsEnumerable().Cast<object>()
                }
            };

            // 帳票出力
            var result = await reportService.OutputAsync(request, ct);
            if (!result.IsSuccessful)
            {
                var message = string.Join(", ", result.ValidationErrors);
                logger.LogWarning(
                    "[{JobId}] {ExecutionId} SAMPLE-W001 {Description}出力失敗 ProcessDate={ProcessDate:yyyy-MM-dd} ValidationErrors={ValidationErrors}",
                    JobId,
                    context.JobExecutionId,
                    Description,
                    context.ProcessDate,
                    message);
                return metrics.ToResult(false, $"帳票出力失敗: {message}");
            }

            metrics.IncrementWrite(readCount);

            // 明細出力タイプのサンプル出力
            var detailRequest = new ReportOutputRequest
            {
                ExecutionId = $"{context.JobExecutionId}-SAMPLE-DETAIL",
                JobExecutionId = context.JobExecutionId,
                ProcessDate = context.ProcessDate,
                TemplateId = DetailTemplateId,
                DataRequest = new ReportDataRequest
                {
                    Mode = ReportDataMode.Streamed,
                    SourceData = sourceQuery.AsEnumerable().Cast<object>()
                }
            };

            var detailResult = await reportService.OutputAsync(detailRequest, ct);
            if (!detailResult.IsSuccessful)
            {
                var message = string.Join(", ", detailResult.ValidationErrors);
                logger.LogWarning(
                    "[{JobId}] {ExecutionId} SAMPLE-W002 {Description}明細出力失敗 ProcessDate={ProcessDate:yyyy-MM-dd} ValidationErrors={ValidationErrors}",
                    JobId,
                    context.JobExecutionId,
                    Description,
                    context.ProcessDate,
                    message);
                return metrics.ToResult(false, $"明細帳票出力失敗: {message}");
            }
            metrics.IncrementWrite(readCount);

            logger.LogInformation(
                "[{JobId}] {ExecutionId} SAMPLE-I002 {Description}完了 読込件数={ReadCount} 出力件数={WriteCount} OutputPath={OutputPath}",
                JobId,
                context.JobExecutionId,
                Description,
                readCount,
                readCount,
                result.OutputFilePath);
            return metrics.ToResult(message: $"出力完了: {result.OutputFilePath}");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[{JobId}] {ExecutionId} SAMPLE-E001 {Description}異常終了 ProcessDate={ProcessDate:yyyy-MM-dd}",
                JobId,
                context.JobExecutionId,
                Description,
                context.ProcessDate);
            return metrics.ToResult(false, ex.Message);
        }
    }
}
