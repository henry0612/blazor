using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace UnifiedAccount.Batch.Framework.Resilience;

/// <summary>
/// FTP/Web通信ジョブ用 Pollyリトライポリシー定義
/// 対象ジョブ: KOZ815 (FTPダウンロード), KOZ820 (FTPアップロード),
///             KOZ830 (Web受信), KOZ835 (Web送信),
///             KOZ850 (ファイル転送), KOZ851 (ファイル転送リトライ)
/// </summary>
public static class RetryPolicies
{
    /// <summary>
    /// 指数バックオフ付き HTTP リトライポリシー
    /// 対象エラー: HttpRequestException / 5xx / 408 / 429
    /// 待機時間: 2^attempt 秒 + 最大300ms ジッター (例: ~2s, ~4s, ~8s)
    /// </summary>
    /// <param name="logger">ロガーを指定する。</param>
    /// <param name="clientName">クライアント名称を指定する。</param>
    /// <param name="retryCount">リトライ件数を指定する。</param>
    public static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy(
        ILogger logger,
        string clientName,
        int retryCount = 3)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                              + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 300)),
                onRetry: (outcome, timeSpan, attempt, context) =>
                {
                    var reason = outcome.Exception?.Message
                        ?? $"HTTP {(int?)outcome.Result?.StatusCode} {outcome.Result?.ReasonPhrase}";
                    logger.LogWarning(
                        "[{ClientName}] HTTP通信リトライ {Attempt}/{RetryCount}: 原因={Reason}, 待機={Delay:F1}s",
                        clientName, attempt, retryCount, reason, timeSpan.TotalSeconds);
                });
    }

    /// <summary>
    /// サーキットブレーカーポリシー
    /// 5連続失敗でブレーク → 30秒後ハーフオープン → 成功でクローズ
    /// </summary>
    /// <param name="logger">ロガーを指定する。</param>
    /// <param name="clientName">クライアント名称を指定する。</param>
    /// <param name="failureThreshold">失敗閾値を指定する。</param>
    /// <param name="breakDurationSeconds">中断経過時間秒数を指定する。</param>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
        ILogger? logger = null,
        string clientName = "",
        int failureThreshold = 5,
        int breakDurationSeconds = 30)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: failureThreshold,
                durationOfBreak: TimeSpan.FromSeconds(breakDurationSeconds),
                onBreak: (outcome, duration) =>
                {
                    var reason = outcome.Exception?.Message
                        ?? $"HTTP {(int?)outcome.Result?.StatusCode}";
                    logger?.LogError(
                        "[{ClientName}] サーキットブレーカーOPEN: 原因={Reason}, 遮断時間={Duration}s",
                        clientName, reason, duration.TotalSeconds);
                },
                onReset: () =>
                {
                    logger?.LogInformation(
                        "[{ClientName}] サーキットブレーカーCLOSED (リセット)", clientName);
                },
                onHalfOpen: () =>
                {
                    logger?.LogInformation(
                        "[{ClientName}] サーキットブレーカーHALF-OPEN (テスト送信中)", clientName);
                });
    }
}




