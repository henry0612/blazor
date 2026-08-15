namespace UnifiedAccount.Application.Services;

/// <summary>
/// マスタ保守サービス (KOZ100-175)
/// </summary>
public interface IMasterMaintenanceService
{
    Task ProcessCompanyChangeAsync(long requestId, CancellationToken ct = default);
    Task ProcessContractChangeAsync(long requestId, CancellationToken ct = default);
    Task ProcessBankBranchChangeAsync(long requestId, CancellationToken ct = default);
}

