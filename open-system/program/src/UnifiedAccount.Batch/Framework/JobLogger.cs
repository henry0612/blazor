using Microsoft.Extensions.Logging;

namespace UnifiedAccount.Batch.Framework;

/// <summary>
/// ジョブ専用ログラッパー
/// </summary>
public class JobLogger
{
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger _logger;
    /// <summary>
    /// ジョブIDを保持する。
    /// </summary>
    private readonly string _jobId;

    public JobLogger(ILogger logger, string jobId)
    {
        _logger = logger;
        _jobId = jobId;
    }

    public void Info(string message, params object[] args)
    {
        _logger.LogInformation($"[{_jobId}] {message}", args);
    }

    public void Warn(string message, params object[] args)
    {
        _logger.LogWarning($"[{_jobId}] {message}", args);
    }

    public void Error(string message, params object[] args)
    {
        _logger.LogError($"[{_jobId}] {message}", args);
    }

    public void Error(Exception ex, string message, params object[] args)
    {
        _logger.LogError(ex, $"[{_jobId}] {message}", args);
    }
}

