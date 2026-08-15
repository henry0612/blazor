namespace UnifiedAccount.Domain.Constants;

/// <summary>
/// `TM_BatchParameters.JobExecutionStatus` に格納するジョブ実行状態。
/// `Failed` を除き、定数名は 184-コード定義書 §2.13.7「バッチ実行状態（JobExecutionStatus）」の表示名に対応する。
/// </summary>
public static class JobExecutionStatuses
{
    /// <summary>
    /// 未実行。空文字および null も未実行として扱う。
    /// </summary>
    public const string NotStarted = "0";

    /// <summary>
    /// 実行中。
    /// </summary>
    public const string Running = "1";

    /// <summary>
    /// 完了。
    /// </summary>
    public const string Completed = "2";

    /// <summary>
    /// 異常終了。
    /// </summary>
    /// <remarks>
    /// 184-コード定義書 §2.13.7 はバッチ実行状態のエラーを `9` と定めるが、現行実装は `3` を書き込む。
    /// 値の是正は既存データの互換性に影響するため、リテラルの定数化とは切り離して判断する。
    /// </remarks>
    public const string Failed = "3";
}
