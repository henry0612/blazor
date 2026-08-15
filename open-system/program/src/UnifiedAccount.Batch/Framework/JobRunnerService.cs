using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UnifiedAccount.Batch.Framework;

/// <summary>
/// ジョブ実行サービス
/// </summary>
public class JobRunnerService : IJobRunner
{
    /// <summary>
    /// サービス一覧を保持する。
    /// </summary>
    private readonly IServiceProvider _services;
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<JobRunnerService> _logger;

    public JobRunnerService(IServiceProvider services, ILogger<JobRunnerService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task<JobResult> RunAsync<TJob>(JobContext context, CancellationToken ct = default) where TJob : IJob
    {
        var job = _services.GetRequiredService<TJob>();
        return await ExecuteJob(job, context, ct);
    }

    public async Task<JobResult> RunAsync(string jobId, JobContext context, CancellationToken ct = default)
    {
        var jobs = _services.GetServices<IJob>();
        var job = jobs.FirstOrDefault(j => j.JobId == jobId)
            ?? throw new InvalidOperationException($"Job '{jobId}' not found.");
        return await ExecuteJob(job, context, ct);
    }

    private async Task<JobResult> ExecuteJob(IJob job, JobContext context, CancellationToken ct)
    {
        _logger.LogInformation("=== Job START: {JobId} ({Description}) ===", job.JobId, job.Description);
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await job.ExecuteAsync(context, ct);
            sw.Stop();

            if (result.Success)
                _logger.LogInformation("=== Job OK: {JobId} R={Read} W={Write} E={Error} in {Elapsed}ms ===",
                    job.JobId, result.ReadCount, result.WriteCount, result.ErrorCount, sw.ElapsedMilliseconds);
            else
                _logger.LogError("=== Job FAILED: {JobId} Message={Message} ===", job.JobId, result.Message);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "=== Job EXCEPTION: {JobId} ===", job.JobId);
            return JobResult.Fail($"Exception: {ex.Message}");
        }
    }
}

