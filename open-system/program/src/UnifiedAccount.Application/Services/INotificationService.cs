namespace UnifiedAccount.Application.Services;

/// <summary>
/// 通知サービス (KOZM00-60)
/// </summary>
public interface INotificationService
{
    Task SendNotificationAsync(string companyCode, string messageCode, string body, CancellationToken ct = default);
    Task<IReadOnlyList<DTOs.NotificationDto>> GetNotificationsAsync(string companyCode, CancellationToken ct = default);
}

