namespace UnifiedAccount.Application.Services;

/// <summary>
/// 帳票/集計サービス (KOZ200-305)
/// </summary>
public interface IReportService
{
    Task GenerateCompanySummaryAsync(DateOnly date, string companyCode, CancellationToken ct = default);
    Task GenerateInstitutionSummaryAsync(DateOnly date, CancellationToken ct = default);
    Task GenerateFeeAggregationAsync(DateOnly date, CancellationToken ct = default);
    Task GenerateCoverLetterAsync(DateOnly date, string companyCode, CancellationToken ct = default);
}

