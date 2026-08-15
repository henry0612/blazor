namespace UnifiedAccount.Application.Services;

/// <summary>
/// 全銀連携サービス (KOZ400-440)
/// </summary>
public interface IZenginService
{
    Task SendZenginDataAsync(DateOnly withdrawalDate, CancellationToken ct = default);
    Task ReceiveZenginResultAsync(DateOnly withdrawalDate, CancellationToken ct = default);
    Task VerifyZenginDataAsync(long batchId, CancellationToken ct = default);
}

