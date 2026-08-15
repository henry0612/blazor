using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Batch.Framework;

/// <summary>
/// ワークフロー実行結果（複数ジョブ結果の集約）
/// </summary>
public record WorkflowResult
{
    /// <summary>
    /// ジョブ結果一覧を取得または設定する。
    /// </summary>
    public IReadOnlyList<JobResult> JobResults { get; }
    /// <summary>
    /// 全ジョブが成功したかどうかを示す。
    /// </summary>
    public bool Success => JobResults.All(r => r.Success);
    /// <summary>
    /// 全ジョブの読込件数合計を取得する。
    /// </summary>
    public int TotalReadCount => JobResults.Sum(r => r.ReadCount);
    /// <summary>
    /// 全ジョブの書込件数合計を取得する。
    /// </summary>
    public int TotalWriteCount => JobResults.Sum(r => r.WriteCount);
    /// <summary>
    /// 全ジョブのエラー件数合計を取得する。
    /// </summary>
    public int TotalErrorCount => JobResults.Sum(r => r.Success ? 0 : r.ErrorCount > 0 ? r.ErrorCount : 1);
    /// <summary>
    /// 全ジョブの処理時間合計を取得する。
    /// </summary>
    public TimeSpan TotalElapsed => TimeSpan.FromTicks(JobResults.Sum(r => (r.Duration ?? TimeSpan.Zero).Ticks));
    public int JobCount => JobResults.Count;
    /// <summary>
    /// 成功したジョブ件数を取得する。
    /// </summary>
    public int SuccessCount => JobResults.Count(r => r.Success);
    /// <summary>
    /// 失敗したジョブ件数を取得する。
    /// </summary>
    public int FailedCount => JobResults.Count(r => !r.Success);

    public WorkflowResult(IEnumerable<JobResult> results)
    {
        JobResults = results.ToList().AsReadOnly();
    }

    /// <summary>
    /// ワークフロー結果をジョブ結果へ変換する。
    /// </summary>
    public JobResult ToJobResult()
    {
        if (Success)
        {
            return JobResult.Ok(
                readCount: TotalReadCount,
                writeCount: TotalWriteCount,
                message: Summary);
        }

        return new JobResult
        {
            ReadCount = TotalReadCount,
            WriteCount = TotalWriteCount,
            ErrorCount = TotalErrorCount,
            Success = false,
            Message = Summary,
            CompletedAt = LocalDateTimeProvider.Now,
        };
    }

    public string Summary =>
        $"Jobs={JobCount} (OK={SuccessCount} NG={FailedCount}) R={TotalReadCount} W={TotalWriteCount} E={TotalErrorCount} in {TotalElapsed.TotalSeconds:F1}s";
}

