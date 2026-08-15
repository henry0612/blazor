namespace UnifiedAccount.Application.Services;

/// <summary>
/// 振替処理サービス (KOZ000-096)
/// </summary>
public interface ITransferService
{
    Task ProcessTransferAsync(DateOnly withdrawalDate, string companyCode, CancellationToken ct = default);
    Task ProcessTransferResultAsync(DateOnly withdrawalDate, CancellationToken ct = default);
    Task CreateTransferDataAsync(DateOnly withdrawalDate, string companyCode, CancellationToken ct = default);
}

