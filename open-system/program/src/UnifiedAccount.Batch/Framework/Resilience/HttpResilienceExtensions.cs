using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UnifiedAccount.Batch.Framework.Resilience;

/// <summary>
/// FTP/Web通信ジョブ用 Pollyリジリエンス対応 HTTP クライアント登録拡張
/// appsettings.json の "Resilience" セクションで動作設定が可能。
/// </summary>
public static class HttpResilienceExtensions
{
    public static IServiceCollection AddResilientHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var retryCount = configuration.GetValue("Resilience:RetryCount", 3);
        var timeoutSeconds = configuration.GetValue("Resilience:TimeoutSeconds", 30);
        var cbFailureThreshold = configuration.GetValue("Resilience:CircuitBreakerFailureThreshold", 5);
        var cbBreakDurationSec = configuration.GetValue("Resilience:CircuitBreakerBreakDurationSeconds", 30);
        var fileTransferBaseUrl = configuration["ApiSettings:FileTransferBaseUrl"] ?? "http://localhost";
        var webApiBaseUrl = configuration["ApiSettings:WebApiBaseUrl"] ?? "http://localhost";

        // ---- サーキットブレーカーはクライアント毎にシングルトンで共有 (状態を持つため) ----
        // ロガーはここでは遅延評価できないため、CB作成時はnull (ログはリトライ側で行う)
        var fileTransferCb = RetryPolicies.GetCircuitBreakerPolicy(
            clientName: "FileTransfer",
            failureThreshold: cbFailureThreshold,
            breakDurationSeconds: cbBreakDurationSec);

        var webApiCb = RetryPolicies.GetCircuitBreakerPolicy(
            clientName: "WebApi",
            failureThreshold: cbFailureThreshold,
            breakDurationSeconds: cbBreakDurationSec);

        // ---- FileTransfer クライアント (KOZ815/KOZ820/KOZ850/KOZ851) ----
        // HttpClient.Timeout は Polly が全リトライを完了できる十分な時間に設定
        services
            .AddHttpClient("FileTransfer", client =>
            {
                client.BaseAddress = new Uri(fileTransferBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds * (retryCount + 1) + 10);
            })
            .AddPolicyHandler((sp, _) =>
            {
                // リトライポリシーはリクエスト毎に生成 (stateless なのでOK)
                var logger = sp.GetRequiredService<ILoggerFactory>()
                               .CreateLogger("UnifiedAccount.Batch.Http.FileTransfer");
                return RetryPolicies.GetHttpRetryPolicy(logger, "FileTransfer", retryCount);
            })
            .AddPolicyHandler(fileTransferCb);

        // ---- WebApi クライアント (KOZ830/KOZ835) ----
        services
            .AddHttpClient("WebApi", client =>
            {
                client.BaseAddress = new Uri(webApiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds * (retryCount + 1) + 10);
            })
            .AddPolicyHandler((sp, _) =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>()
                               .CreateLogger("UnifiedAccount.Batch.Http.WebApi");
                return RetryPolicies.GetHttpRetryPolicy(logger, "WebApi", retryCount);
            })
            .AddPolicyHandler(webApiCb);

        return services;
    }
}
