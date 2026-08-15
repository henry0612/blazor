namespace UnifiedAccount.Domain.Enums;

/// <summary>
/// 契約ステータス
/// </summary>
public enum ContractStatus
{
    /// <summary>
    /// 有効
    /// </summary>
    Active = 0,
    /// <summary>
    /// 中止
    /// </summary>
    Suspended = 1,
    /// <summary>
    /// 抹消
    /// </summary>
    Deleted = 9
}

