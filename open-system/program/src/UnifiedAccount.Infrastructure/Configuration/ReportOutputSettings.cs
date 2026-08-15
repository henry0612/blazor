namespace UnifiedAccount.Infrastructure.Configuration;

/// <summary>
/// 帳票出力設定
/// </summary>
public class ReportSettings
{
    public const string SectionName = "Report";

    /// <summary>
    /// 帳票出力先基底フォルダ。
    /// 未指定時は実行ディレクトリ配下の "output/reports" を使用する。
    /// </summary>
    public string OutputFolder { get; set; } = Path.Combine("output", "reports");

    /// <summary>
    /// 帳票テンプレートフォルダ。
    /// 未指定時は ReportingDependencyInjection の既定探索順で決定する。
    /// </summary>
    public string? TemplateFolder { get; set; }
}



