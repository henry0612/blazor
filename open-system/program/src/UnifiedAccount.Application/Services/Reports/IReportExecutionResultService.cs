using UnifiedAccount.Domain.Entities.Reports;

namespace UnifiedAccount.Application.Services.Reports;

/// <summary>
/// 帳票実行履歴管理サービス（ステップ6-7）
/// 実行追跡、リトライ管理、監査ログを提供
/// </summary>
public interface IReportExecutionResultService
{
    /// <summary>
    /// 実行記録を開始（ステップ6: 実行開始）
    /// </summary>
    Task<ReportExecution> StartExecutionAsync(
        string executionId,
        string? jobExecutionId,
        string templateId,
        string outputMode,
        string? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 実行記録を成功状態で終了（ステップ6: 実行完了）
    /// </summary>
    Task CompleteExecutionAsync(
        string executionId,
        string outputFilePath,
        bool isPartialOutput = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 実行記録をエラー状態で終了
    /// </summary>
    Task FailExecutionAsync(
        string executionId,
        Exception exception,
        string? incompleteFilePath = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// リトライ可否判定＆リトライ実行（ステップ7: リトライ管理）
    /// 戻り値: true = リトライ可能、false = 最大回数超過
    /// </summary>
    Task<bool> CanRetryAsync(
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// リトライ回数をインクリメント
    /// </summary>
    Task IncrementRetryCountAsync(
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 実行記録を取得
    /// </summary>
    Task<ReportExecution?> GetExecutionAsync(
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 特定テンプレートの最新実行記録を取得
    /// </summary>
    Task<ReportExecution?> GetLatestExecutionByTemplateAsync(
        string templateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 未完了ファイル（.incomplete）のクリーンアップ
    /// </summary>
    Task CleanupIncompleteFilesAsync(
        int retentionDays = 7,
        CancellationToken cancellationToken = default);
}
