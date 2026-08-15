namespace UnifiedAccount.Reporting.Models;

/// <summary>
/// 帳票テンプレート名定数 (エンジン非依存)。
/// 各エンジンプロバイダはこの定数を使ってテンプレートを実装する。
/// </summary>
public static class ReportTemplates
{
    /// <summary>
    /// 送付状 (宛先/送付者/日付/内容テーブル)
    /// </summary>
    public const string CoverLetter = "CoverLetter";

    /// <summary>
    /// 通知書 (宛先/見出し/本文/備考)
    /// </summary>
    public const string Notification = "Notification";

    /// <summary>
    /// 会計伝票 (仕訳番号/借方・貸方テーブル/合計)
    /// </summary>
    public const string AccountingSlip = "AccountingSlip";
}

