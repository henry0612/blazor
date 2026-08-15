namespace UnifiedAccount.Domain.Constants;

/// <summary>
/// 帳票実行履歴の実行ステータス。
/// 値は 222 帳票実行結果管理 共通プログラム定義書の状態一覧と1対1で対応する。
/// </summary>
public static class ReportExecutionStatuses
{
    /// <summary>
    /// 実行中。
    /// </summary>
    public const string Running = "Running";

    /// <summary>
    /// 一部出力。出力対象の一部だけが生成された状態。
    /// </summary>
    public const string PartialOutput = "PartialOutput";

    /// <summary>
    /// 正常終了。
    /// </summary>
    public const string Succeeded = "Succeeded";

    /// <summary>
    /// 異常終了。
    /// </summary>
    public const string Failed = "Failed";
}
