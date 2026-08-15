namespace UnifiedAccount.Batch.Framework;

/// <summary>
/// 全バッチジョブの共通インタフェース
/// </summary>
public interface IJob
{
    /// <summary>
    /// ジョブID (例: "KOZ010")
    /// </summary>
    string JobId { get; }

    /// <summary>
    /// ジョブ説明
    /// </summary>
    string Description { get; }

    /// <summary>
    /// ジョブ実行
    /// </summary>
    Task<JobResult> ExecuteAsync(JobContext context, CancellationToken ct = default);
}

