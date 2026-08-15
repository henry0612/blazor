namespace UnifiedAccount.Reporting.Models;

/// <summary>
/// 固定レイアウト帳票の定義 (送付状・通知書・会計伝票など)
/// </summary>
/// <param name="ReportId">帳票IDを指定する。</param>
/// <param name="TemplateName">テンプレート識別名を指定する。<see cref="ReportTemplates"/> の定数を使用する。</param>
/// <param name="Pages">ページ一覧を指定する。</param>
public sealed record FormReportDefinition(
    string ReportId,
    string TemplateName,
    IReadOnlyList<FormPage> Pages);

/// <summary>
/// 帳票1ページ分のフィールドデータ (キー/値マップ)
/// </summary>
/// <param name="Fields">項目一覧を指定する。</param>
public sealed record FormPage(IReadOnlyDictionary<string, object?> Fields);

/// <summary>
/// 固定レイアウト帳票のレンダリング結果
/// </summary>
/// <param name="OutputPath">出力パスを指定する。</param>
/// <param name="Format">形式を指定する。</param>
/// <param name="PageCount">ページ件数を指定する。</param>
public sealed record FormRenderResult(string OutputPath, string Format, int PageCount);




