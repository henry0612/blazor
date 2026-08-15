using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Batch.Framework;

/// <summary>
/// ジョブ実行結果 (RETURN-CODE 相当)
/// </summary>
public class JobResult
{
    /// <summary>
    /// 読込件数 (READ-CNT)
    /// </summary>
    public int ReadCount { get; set; }

    /// <summary>
    /// 書込件数 (WRITE-CNT)
    /// </summary>
    public int WriteCount { get; set; }

    /// <summary>
    /// エラー件数 (ERROR-CNT)
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// スキップ件数
    /// </summary>
    public int SkipCount { get; set; }

    /// <summary>
    /// 成功フラグ (RETURN-CODE == 0)
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// メッセージ
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 処理開始時刻
    /// </summary>
    public DateTime StartedAt { get; set; } = LocalDateTimeProvider.Now;

    /// <summary>
    /// 処理終了時刻
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 処理時間
    /// </summary>
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;

    public static JobResult Ok(int readCount = 0, int writeCount = 0, string? message = null)
    {
        return new JobResult
        {
            ReadCount = readCount,
            WriteCount = writeCount,
            Success = true,
            Message = message,
            CompletedAt = LocalDateTimeProvider.Now
        };
    }

    public static JobResult Fail(string message, int readCount = 0, int errorCount = 0)
    {
        return new JobResult
        {
            ReadCount = readCount,
            ErrorCount = errorCount,
            Success = false,
            Message = message,
            CompletedAt = LocalDateTimeProvider.Now
        };
    }
}
