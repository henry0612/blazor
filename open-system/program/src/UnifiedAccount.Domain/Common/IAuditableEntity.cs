namespace UnifiedAccount.Domain.Common;

/// <summary>
/// 監査列インタフェース
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
