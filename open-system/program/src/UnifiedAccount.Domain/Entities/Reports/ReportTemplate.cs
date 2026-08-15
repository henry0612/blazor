using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 帳票テンプレートのメタデータ。
/// COBOL移行元: SYMRTN (テンプレート定義マスタ)
/// </summary>
public class ReportTemplate
{
    /// <summary>
    /// テンプレートIDを取得または設定する。
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;
    /// <summary>
    /// テンプレート名称を取得または設定する。
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;
    /// <summary>
    /// テンプレートファイルパスを取得または設定する。
    /// </summary>
    public string TemplateFilePath { get; set; } = string.Empty;
    /// <summary>
    /// 出力形式を取得または設定する。
    /// </summary>
    public string OutputFormat { get; set; } = string.Empty;
    /// <summary>
    /// ページ幅を取得または設定する。
    /// </summary>
    public int PageWidth { get; set; }
    /// <summary>
    /// ページ高さを取得または設定する。
    /// </summary>
    public int PageHeight { get; set; }
    /// <summary>
    /// 必須項目一覧を取得または設定する。
    /// </summary>
    public ICollection<string> RequiredFields { get; set; } = new List<string>();
    /// <summary>
    /// 明細項目一覧を取得または設定する。
    /// </summary>
    public ICollection<string> DetailFields { get; set; } = new List<string>();
    /// <summary>
    /// 集計項目一覧を取得または設定する。
    /// </summary>
    public ICollection<string> SummaryFields { get; set; } = new List<string>();
    /// <summary>
    /// 作成日時を取得または設定する。
    /// </summary>
    public DateTime CreatedAt { get; set; } = LocalDateTimeProvider.Now;
    /// <summary>
    /// 更新日時を取得または設定する。
    /// </summary>
    public DateTime UpdatedAt { get; set; } = LocalDateTimeProvider.Now;
    /// <summary>
    /// 有効状態を取得または設定する。
    /// </summary>
    public bool IsActive { get; set; }

    public ReportTemplate()
    {
        RequiredFields = new List<string>();
        DetailFields = new List<string>();
        SummaryFields = new List<string>();
    }
}
