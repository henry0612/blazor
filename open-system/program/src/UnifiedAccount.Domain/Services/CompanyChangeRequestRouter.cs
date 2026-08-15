using UnifiedAccount.Domain.Enums;

namespace UnifiedAccount.Domain.Services;

/// <summary>
/// 会社異動リクエストのルーティング判定
/// </summary>
public static class CompanyChangeRequestRouter
{
    /// <summary>
    /// ルートを判定する。
    /// </summary>
    public static CompanyChangeRequestRoute Resolve(string requestType, string? cban)
    {
        var normalizedRequestType = requestType.Trim();
        var normalizedCban = cban?.Trim();

        return normalizedRequestType switch
        {
            "11" => CompanyChangeRequestRoute.CompanyBasic11,
            "12" => CompanyChangeRequestRoute.CompanyAddress12,
            "13" => CompanyChangeRequestRoute.CompanyProcessing13,
            "14" => CompanyChangeRequestRoute.CompanyFee14,
            "15" => CompanyChangeRequestRoute.CompanyTransfer15,
            "16" or "17" or "18" or "19" => CompanyChangeRequestRoute.CompanyType16To19,
            "71" when normalizedCban == "1" => CompanyChangeRequestRoute.CompanyKanjiName71,
            "71" when normalizedCban == "2" => CompanyChangeRequestRoute.CompanyKanjiAddress71,
            "71" when normalizedCban == "3" => CompanyChangeRequestRoute.CompanyKanjiContact71,
            _ => CompanyChangeRequestRoute.Unsupported
        };
    }

    /// <summary>
    /// 対象ルートかどうかを判定する。
    /// </summary>
    public static bool IsSupported(string requestType, string? cban)
    {
        return Resolve(requestType, cban) != CompanyChangeRequestRoute.Unsupported;
    }
}
