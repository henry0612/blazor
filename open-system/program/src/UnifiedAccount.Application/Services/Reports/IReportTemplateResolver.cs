using UnifiedAccount.Domain.Entities.Reports;

namespace UnifiedAccount.Application.Services.Reports;

/// <summary>
/// 帳票テンプレートの検索と検証を行うサービス（ステップ2）。
/// COBOL移行元: KSYMZERO (テンプレート検証ロジック)
/// </summary>
public interface IReportTemplateResolver
{
    /// <summary>
    /// テンプレートIDからテンプレート定義を検索し、ファイルの存在と妥当性を検証する。
    /// </summary>
    Task<ReportTemplate?> ResolveAsync(string templateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 複数のテンプレートを一括取得（キャッシュ想定）。
    /// </summary>
    Task<IEnumerable<ReportTemplate>> ResolveAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// テンプレートが使用可能状態か確認。
    /// </summary>
    Task<bool> IsValidAsync(string templateId, CancellationToken cancellationToken = default);
}
