namespace UnifiedAccount.Batch.Framework;

/// <summary>
/// ジョブ実行サービスインタフェース
/// </summary>
public interface IJobRunner
{
    Task<JobResult> RunAsync<TJob>(JobContext context, CancellationToken ct = default) where TJob : IJob;
    Task<JobResult> RunAsync(string jobId, JobContext context, CancellationToken ct = default);
}

