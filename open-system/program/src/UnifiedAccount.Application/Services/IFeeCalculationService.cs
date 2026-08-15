namespace UnifiedAccount.Application.Services;

/// <summary>
/// 手数料計算サービス (KOZ700-760)
/// </summary>
public interface IFeeCalculationService
{
    Task<decimal> CalculateFeeAsync(string companyCode, int transferCount, CancellationToken ct = default);
    Task ProcessMonthlyFeeAsync(DateOnly yearMonth, CancellationToken ct = default);
}

