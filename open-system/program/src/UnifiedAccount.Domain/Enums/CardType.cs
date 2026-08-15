namespace UnifiedAccount.Domain.Enums;

/// <summary>
/// カード区分
/// </summary>
public enum CardType
{
    /// <summary>
    /// 口座(カード1)
    /// </summary>
    Account = 81,
    /// <summary>
    /// 住所(カード2)
    /// </summary>
    Address = 82,
    /// <summary>
    /// 金額(カード5)
    /// </summary>
    Amount = 83,
    /// <summary>
    /// クレジット(カード6)
    /// </summary>
    Credit = 84
}

