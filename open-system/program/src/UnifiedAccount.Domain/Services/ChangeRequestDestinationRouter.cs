using UnifiedAccount.Domain.Enums;

namespace UnifiedAccount.Domain.Services;

/// <summary>
/// IF-01異動データの格納先判定
/// </summary>
public static class ChangeRequestDestinationRouter
{
    public static ChangeRequestDestination Resolve(string requestType, string? cban)
    {
        var normalizedRequestType = requestType.Trim();
        var normalizedCban = cban?.Trim();

        return normalizedRequestType switch
        {
            "01" => ChangeRequestDestination.Bank,
            "11" or "12" or "13" or "14" or "15" or "16" or "17" or "18" or "19"
                => ChangeRequestDestination.Company,
            "20" => ChangeRequestDestination.AccountNumber,
            "21" or "22" or "23" or "24" => ChangeRequestDestination.Contract,
            "71" when normalizedCban is "1" or "2" or "3" => ChangeRequestDestination.Company,
            "72" when normalizedCban is not null
                && int.TryParse(normalizedCban, out var messageCban)
                && messageCban is >= 1 and <= 13 => ChangeRequestDestination.Message,
            _ => ChangeRequestDestination.Unsupported
        };
    }
}
