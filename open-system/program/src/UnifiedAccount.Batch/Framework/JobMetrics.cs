using System.Diagnostics;

using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Batch.Framework;

/// <summary>
/// ジョブ処理メトリクス
/// </summary>
public class JobMetrics
{
    /// <summary>
    /// ストップウォッチを保持する。
    /// </summary>
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    /// <summary>
    /// 読込件数を保持する。
    /// </summary>
    private int _readCount;
    /// <summary>
    /// 書込件数を保持する。
    /// </summary>
    private int _writeCount;
    /// <summary>
    /// エラー件数を保持する。
    /// </summary>
    private int _errorCount;
    /// <summary>
    /// スキップ件数を保持する。
    /// </summary>
    private int _skipCount;

    public int ReadCount => _readCount;
    public int WriteCount => _writeCount;
    public int ErrorCount => _errorCount;
    public int SkipCount => _skipCount;
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// 読込件数を加算する。
    /// </summary>
    /// <param name="count">加算する件数を指定する。</param>
    public void IncrementRead(int count = 1) => Interlocked.Add(ref _readCount, count);
    /// <summary>
    /// 書込件数を加算する。
    /// </summary>
    /// <param name="count">加算する件数を指定する。</param>
    public void IncrementWrite(int count = 1) => Interlocked.Add(ref _writeCount, count);
    /// <summary>
    /// エラー件数を加算する。
    /// </summary>
    /// <param name="count">加算する件数を指定する。</param>
    public void IncrementError(int count = 1) => Interlocked.Add(ref _errorCount, count);
    /// <summary>
    /// スキップ件数を加算する。
    /// </summary>
    /// <param name="count">加算する件数を指定する。</param>
    public void IncrementSkip(int count = 1) => Interlocked.Add(ref _skipCount, count);

    public JobResult ToResult(bool success = true, string? message = null)
    {
        _stopwatch.Stop();
        return new JobResult
        {
            ReadCount = _readCount,
            WriteCount = _writeCount,
            ErrorCount = _errorCount,
            SkipCount = _skipCount,
            Success = success,
            Message = message,
            CompletedAt = LocalDateTimeProvider.Now
        };
    }
}

