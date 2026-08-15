namespace UnifiedAccount.Domain.Interfaces;

/// <summary>
/// 排他制御サービスインタフェース (M9SYRCH/M9SYRCL/M9SYRUP 相当)
/// </summary>
public interface IJobExecutionGuard
{
    /// <summary>
    /// 指定 BatchParameter レコードが実行可能かをチェックする (true=実行可能)
    /// </summary>
    Task<bool> CheckAsync(long batchParameterId, CancellationToken ct = default);

    /// <summary>
    /// 指定 BatchParameter を実行中に設定して取得する. 取得できなければ false を返す。
    /// </summary>
    Task<bool> AcquireAsync(long batchParameterId, string? owner = null, CancellationToken ct = default);

    /// <summary>
    /// 指定 BatchParameter の実行ロックを解除し結果を記録する。
    /// </summary>
    Task ReleaseAsync(long batchParameterId, bool success, CancellationToken ct = default);
}

