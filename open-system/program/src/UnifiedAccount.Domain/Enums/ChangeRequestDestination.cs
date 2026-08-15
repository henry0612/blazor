namespace UnifiedAccount.Domain.Enums;

/// <summary>
/// IF-01異動データの格納先
/// </summary>
public enum ChangeRequestDestination
{
    Unsupported = 0,
    Bank,
    Company,
    AccountNumber,
    Contract,
    Message
}
