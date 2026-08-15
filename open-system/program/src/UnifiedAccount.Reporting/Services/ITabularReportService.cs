using UnifiedAccount.Reporting.Models;

namespace UnifiedAccount.Reporting.Services;

/// <summary>
/// 一覧帳票レンダリングの内部契約。
/// 業務ジョブの公開入口は IReportOutputService を優先する。
/// </summary>
public interface ITabularReportService
{
    Task<ReportRenderResult> RenderAsync(
        TabularReportDefinition definition,
        string outputPath,
        CancellationToken ct = default);
}
