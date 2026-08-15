using UnifiedAccount.Reporting.Models;

namespace UnifiedAccount.Reporting.Services;

/// <summary>
/// 固定レイアウト帳票（送付状・通知書・会計伝票）のレンダリングサービスインターフェース。
/// 実装は UnifiedAccount.Reporting.CoReports プロバイダに配置する。
/// 本契約はレンダラ内部契約であり、業務ジョブの公開入口は IReportOutputService を優先する。
/// </summary>
public interface IFormReportService
{
    Task<FormRenderResult> RenderAsync(
        FormReportDefinition definition,
        string outputPath,
        CancellationToken ct = default);
}
