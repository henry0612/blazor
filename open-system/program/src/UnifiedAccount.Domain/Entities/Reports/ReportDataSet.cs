using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// Buffered モード用のデータセット。
/// 全データをメモリに保持（≤10k行）。
/// </summary>
public class ReportDataSet
{
    /// <summary>
    /// テンプレートIDを取得または設定する。
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;
    /// <summary>
    /// ヘッダーデータを取得または設定する。
    /// </summary>
    public Dictionary<string, object> HeaderData { get; set; } = new();
    /// <summary>
    /// 明細行一覧を取得または設定する。
    /// </summary>
    public List<Dictionary<string, object>> DetailRows { get; set; } = new();
    /// <summary>
    /// 集計データを取得または設定する。
    /// </summary>
    public Dictionary<string, object> SummaryData { get; set; } = new();
    /// <summary>
    /// 生成日時を取得または設定する。
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    public ReportDataSet()
    {
        HeaderData = new Dictionary<string, object>();
        DetailRows = new List<Dictionary<string, object>>();
        SummaryData = new Dictionary<string, object>();
        GeneratedAt = LocalDateTimeProvider.Now;
    }

    public int GetTotalRowCount()
    {
        return DetailRows?.Count ?? 0;
    }
}
