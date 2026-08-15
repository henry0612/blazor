namespace UnifiedAccount.Reporting.Models;

/// <summary>
/// 帳票の列定義を表すレコード。
/// </summary>
/// <param name="Key">キーを指定する。</param>
/// <param name="Header">ヘッダーを指定する。</param>
/// <param name="Width">幅を指定する。</param>
public sealed record ReportColumn(string Key, string Header, float Width = 110f);

/// <summary>
/// 表形式帳票の定義を表すレコード。
/// </summary>
/// <param name="ReportId">帳票IDを指定する。</param>
/// <param name="Title">タイトルを指定する。</param>
/// <param name="Columns">列一覧を指定する。</param>
/// <param name="Rows">行一覧を指定する。</param>
/// <param name="SubTitle">副次タイトルを指定する。</param>
/// <param name="ShowPageNumbers">ページ番号を表示するかどうかを指定する。</param>
/// <param name="SummaryRows">集計行一覧を指定する。</param>
/// <param name="GroupByColumn">集計単位となる列名を指定する。</param>
public sealed record TabularReportDefinition(
    string ReportId,
    string Title,
    IReadOnlyList<ReportColumn> Columns,
    IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows,
    string? SubTitle = null,
    bool ShowPageNumbers = true,
    IReadOnlyList<IReadOnlyDictionary<string, object?>>? SummaryRows = null,
    string? GroupByColumn = null);

/// <summary>
/// 表形式帳票のレンダリング結果を表すレコード。
/// </summary>
/// <param name="OutputPath">出力パスを指定する。</param>
/// <param name="Format">形式を指定する。</param>
/// <param name="RowCount">行件数を指定する。</param>
public sealed record ReportRenderResult(string OutputPath, string Format, int RowCount);





