using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Batch.Framework;
using UnifiedAccount.Infrastructure.Persistence;

/// <summary>
/// サンプルバッチ
/// </summary>
/// <remarks>
/// バッチ処理のサンプルジョブです。
/// 実行例:
/// UnifiedAccount.Batch.exe --job SampleBatch
/// </remarks>
public class SampleBatch : JobBase
{
    public SampleBatch(ITransactionCoordinator transactionCoordinator, ILoggerFactory loggerFactory, IJobRunner jobRunner)
        : base(transactionCoordinator, loggerFactory, jobRunner)
    {
    }

    public override string JobId => "SampleBatch";
    public override string Description => "サンプルバッチ";

    /// <summary>
    /// ジョブ本体を実行します。
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
        var logger = Logger as ILogger<SampleBatch> ?? context.Services.GetRequiredService<ILogger<SampleBatch>>();
        var db = context.Services.GetRequiredService<AppDbContext>();
        // Job内のカウントに使用するメトリクスの初期化
        var metrics = new JobMetrics();

        logger.LogInformation(
            "[{JobId}] {ExecutionId} SAMPLE-I001 {Description}開始 ProcessDate={ProcessDate:yyyy-MM-dd}",
            JobId,
            context.JobExecutionId,
            Description,
            context.ProcessDate);

        try
        {
            // データ取得
            var targetQuery = db.CompanyMasterChangeLogs
                .Where(e => e.JobExecutionId == context.JobExecutionId)
                .OrderBy(e => e.Id);

            // 件数を取得
            var readCount = await targetQuery.CountAsync(ct);
            // メトリクスに読込件数を記録
            metrics.IncrementRead(readCount);

            if (readCount > 1000000)
            {
                logger.LogWarning(
                    "[{JobId}] {ExecutionId} SAMPLE-W003 {Description}業務失敗終了 ProcessDate={ProcessDate:yyyy-MM-dd} FailureReason={FailureReason}",
                    JobId,
                    context.JobExecutionId,
                    Description,
                    context.ProcessDate,
                    "読込件数上限超過");
                return metrics.ToResult(false, "読込件数上限超過");
            }

            if (readCount == 0)
            {
                logger.LogInformation(
                    "[{JobId}] {ExecutionId} SAMPLE-I002 {Description}正常終了 読込件数={ReadCount} 更新件数={WriteCount}",
                    JobId,
                    context.JobExecutionId,
                    Description,
                    readCount,
                    0);
                return metrics.ToResult(message: "対象データなし");
            }

            // データ更新
            var entityItems = await targetQuery.ToListAsync(ct);
            foreach (var item in entityItems)
            {
                // IsErrorフラグを反転
                item.IsError = !item.IsError;
                logger.LogDebug("[{JobId}] {ExecutionId} Entity更新対象: {CompanyCode}/{RequestType} IsError={IsError}",
                    JobId,
                    context.JobExecutionId,
                    item.CompanyCode,
                    item.RequestType,
                    item.IsError);
            }

            // データ更新を保存（通常はこの方法で更新する）
            await db.SaveChangesAsync(ct);
            // メトリクスに更新件数を記録
            metrics.IncrementWrite(entityItems.Count);

            // 大量データの更新は、ExecuteUpdateAsyncを使用して一括更新することも可能
            var bulkUpdatedCount = await db.CompanyMasterChangeLogs
                .Where(e => e.JobExecutionId == context.JobExecutionId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(e => e.IsError, e => !e.IsError),
                    ct);

            // メトリクスに一括更新件数を記録
            metrics.IncrementWrite(bulkUpdatedCount);

            logger.LogInformation(
                "[{JobId}] {ExecutionId} SAMPLE-I003 {Description}正常終了 読込件数={ReadCount} 更新件数={WriteCount}",
                JobId,
                context.JobExecutionId,
                Description,
                metrics.ReadCount,
                metrics.WriteCount);

            return metrics.ToResult(message: $"処理完了: Entity更新={metrics.ReadCount}件 Bulk更新={metrics.WriteCount}件");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[{JobId}] {ExecutionId} SAMPLE-E003 {Description}異常終了 ProcessDate={ProcessDate:yyyy-MM-dd}",
                JobId,
                context.JobExecutionId,
                Description,
                context.ProcessDate);
            return metrics.ToResult(false, ex.Message);
        }
    }
}
