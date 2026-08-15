namespace UnifiedAccount.Application.Services;

/// <summary>
/// 共済連連携サービス (CHO000-910)
/// </summary>
public interface ICooperativeService
{
    Task ProcessCooperativeTransferAsync(DateOnly date, CancellationToken ct = default);
    Task GenerateBillingAsync(DateOnly date, CancellationToken ct = default);
    Task ProcessSettlementAsync(DateOnly date, CancellationToken ct = default);
}

