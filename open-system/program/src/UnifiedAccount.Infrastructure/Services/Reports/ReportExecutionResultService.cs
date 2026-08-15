using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Domain.Constants;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Application.Services.Reports;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Infrastructure.Services.Reports;

/// <summary>
/// 帳票実行履歴管理サービス実装（ステップ6-7）
/// DB 記録、リトライ判定、ファイルクリーンアップを提供
/// </summary>
public class ReportExecutionResultService : IReportExecutionResultService
{
    /// <summary>
    /// データベースコンテキストを保持する。
    /// </summary>
    private readonly AppDbContext _dbContext;
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<ReportExecutionResultService> _logger;

    public ReportExecutionResultService(
        AppDbContext dbContext,
        ILogger<ReportExecutionResultService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 実行記録を開始（ステップ6: 実行開始）
    /// </summary>
    /// <param name="executionId">実行IDを指定する。</param>
    /// <param name="jobExecutionId">ジョブ実行IDを指定する。</param>
    /// <param name="templateId">テンプレートIDを指定する。</param>
    /// <param name="outputMode">出力モードを指定する。</param>
    /// <param name="userId">ユーザーIDを指定する。</param>
    /// <param name="cancellationToken">cancellationTokenを指定する。</param>
    public async Task<ReportExecution> StartExecutionAsync(
        string executionId,
        string? jobExecutionId,
        string templateId,
        string outputMode,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        var execution = new ReportExecution
        {
            ExecutionId = executionId,
            JobExecutionId = jobExecutionId,
            TemplateId = templateId,
            Status = ReportExecutionStatuses.Running,
            OutputMode = outputMode,
            RetryCount = 0,
            StartTime = LocalDateTimeProvider.Now,
            UserId = userId,
            Notes = "Execution started"
        };

        _dbContext.ReportExecutions.Add(execution);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "実行記録開始: ExecutionId={ExecutionId}, JobExecutionId={JobExecutionId}, TemplateId={TemplateId}, Mode={Mode}",
            executionId, jobExecutionId, templateId, outputMode);

        return execution;
    }

    /// <summary>
    /// 実行記録を成功状態で終了（ステップ6: 実行完了）
    /// </summary>
    /// <param name="executionId">実行IDを指定する。</param>
    /// <param name="outputFilePath">出力ファイルパスを指定する。</param>
    /// <param name="isPartialOutput">一部出力を指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task CompleteExecutionAsync(
        string executionId,
        string outputFilePath,
        bool isPartialOutput = false,
        CancellationToken cancellationToken = default)
    {
        var execution = await _dbContext.ReportExecutions
            .FirstOrDefaultAsync(e => e.ExecutionId == executionId, cancellationToken);

        if (execution == null)
        {
            _logger.LogWarning("実行記録が見つかりません: ExecutionId={ExecutionId}", executionId);
            throw new InvalidOperationException($"Execution not found: {executionId}");
        }

        execution.Status = isPartialOutput ? ReportExecutionStatuses.PartialOutput : ReportExecutionStatuses.Succeeded;
        execution.OutputFilePath = outputFilePath;
        execution.IsPartialOutput = isPartialOutput;
        execution.EndTime = LocalDateTimeProvider.Now;

        if (execution.StartTime.HasValue)
        {
            execution.ElapsedMilliseconds = (long)(execution.EndTime.Value - execution.StartTime.Value).TotalMilliseconds;
        }

        _dbContext.ReportExecutions.Update(execution);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "実行記録完了: ExecutionId={ExecutionId}, Status={Status}, OutputFile={OutputFile}, Elapsed={ElapsedMs}ms",
            executionId, execution.Status, outputFilePath, execution.ElapsedMilliseconds);
    }

    /// <summary>
    /// 実行記録をエラー状態で終了
    /// </summary>
    /// <param name="executionId">実行IDを指定する。</param>
    /// <param name="exception">例外を指定する。</param>
    /// <param name="incompleteFilePath">未完了ファイルパスを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task FailExecutionAsync(
        string executionId,
        Exception exception,
        string? incompleteFilePath = null,
        CancellationToken cancellationToken = default)
    {
        var execution = await _dbContext.ReportExecutions
            .FirstOrDefaultAsync(e => e.ExecutionId == executionId, cancellationToken);

        if (execution == null)
        {
            _logger.LogWarning("実行記録が見つかりません: ExecutionId={ExecutionId}", executionId);
            throw new InvalidOperationException($"Execution not found: {executionId}");
        }

        execution.Status = ReportExecutionStatuses.Failed;
        execution.ErrorMessage = exception.Message;
        execution.ErrorStackTrace = exception.StackTrace;
        execution.IncompleteFilePath = incompleteFilePath;
        execution.IsPartialOutput = !string.IsNullOrEmpty(incompleteFilePath);
        execution.EndTime = LocalDateTimeProvider.Now;

        if (execution.StartTime.HasValue)
        {
            execution.ElapsedMilliseconds = (long)(execution.EndTime.Value - execution.StartTime.Value).TotalMilliseconds;
        }

        _dbContext.ReportExecutions.Update(execution);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogError(
            exception,
            "実行記録エラー: ExecutionId={ExecutionId}, Message={Message}, IncompleteFile={IncompleteFile}",
            executionId, exception.Message, incompleteFilePath);
    }

    /// <summary>
    /// リトライ可否判定＆リトライ実行（ステップ7: リトライ管理）
    /// 戻り値: true = リトライ可能、false = 最大回数超過
    /// </summary>
    /// <param name="executionId">実行IDを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task<bool> CanRetryAsync(
        string executionId,
        CancellationToken cancellationToken = default)
    {
        var execution = await _dbContext.ReportExecutions
            .FirstOrDefaultAsync(e => e.ExecutionId == executionId, cancellationToken);

        if (execution == null)
        {
            _logger.LogWarning("実行記録が見つかりません: ExecutionId={ExecutionId}", executionId);
            return false;
        }

        bool canRetry = execution.RetryCount < execution.MaxRetries;

        _logger.LogInformation(
            "リトライ可否判定: ExecutionId={ExecutionId}, RetryCount={Current}/{Max}, CanRetry={Result}",
            executionId, execution.RetryCount, execution.MaxRetries, canRetry);

        return canRetry;
    }

    /// <summary>
    /// リトライ回数をインクリメント
    /// </summary>
    /// <param name="executionId">実行IDを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task IncrementRetryCountAsync(
        string executionId,
        CancellationToken cancellationToken = default)
    {
        var execution = await _dbContext.ReportExecutions
            .FirstOrDefaultAsync(e => e.ExecutionId == executionId, cancellationToken);

        if (execution == null)
        {
            _logger.LogWarning("実行記録が見つかりません: ExecutionId={ExecutionId}", executionId);
            throw new InvalidOperationException($"Execution not found: {executionId}");
        }

        execution.RetryCount++;
        execution.Status = ReportExecutionStatuses.Running; // リトライ実行開始

        _dbContext.ReportExecutions.Update(execution);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "リトライ回数インクリメント: ExecutionId={ExecutionId}, RetryCount={Count}",
            executionId, execution.RetryCount);
    }

    /// <summary>
    /// 実行記録を取得
    /// </summary>
    /// <param name="executionId">実行IDを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task<ReportExecution?> GetExecutionAsync(
        string executionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ReportExecutions
            .FirstOrDefaultAsync(e => e.ExecutionId == executionId, cancellationToken);
    }

    /// <summary>
    /// 特定テンプレートの最新実行記録を取得
    /// </summary>
    /// <param name="templateId">テンプレートIDを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task<ReportExecution?> GetLatestExecutionByTemplateAsync(
        string templateId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ReportExecutions
            .Where(e => e.TemplateId == templateId)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 未完了ファイル（.incomplete）のクリーンアップ
    /// 指定日数より前の .incomplete ファイルを削除
    /// </summary>
    /// <param name="retentionDays">保持期間日一覧を指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task CleanupIncompleteFilesAsync(
        int retentionDays = 7,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = LocalDateTimeProvider.Now.AddDays(-retentionDays);

        var incompleteExecutions = await _dbContext.ReportExecutions
            .Where(e => !string.IsNullOrEmpty(e.IncompleteFilePath)
                     && e.CreatedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        int deletedCount = 0;
        foreach (var execution in incompleteExecutions)
        {
            try
            {
                if (!string.IsNullOrEmpty(execution.IncompleteFilePath)
                    && File.Exists(execution.IncompleteFilePath))
                {
                    File.Delete(execution.IncompleteFilePath);
                    deletedCount++;

                    _logger.LogInformation(
                        "未完了ファイル削除: {FilePath}",
                        execution.IncompleteFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "未完了ファイル削除エラー: {FilePath}",
                    execution.IncompleteFilePath);
            }
        }

        _logger.LogInformation(
            "未完了ファイルクリーンアップ完了: {DeletedCount} ファイル削除",
            deletedCount);
    }
}


