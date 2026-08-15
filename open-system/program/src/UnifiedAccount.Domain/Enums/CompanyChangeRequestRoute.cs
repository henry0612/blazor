namespace UnifiedAccount.Domain.Enums;

/// <summary>
/// 会社異動リクエストのルート種別
/// </summary>
public enum CompanyChangeRequestRoute
{
    /// <summary>
    /// 対象外
    /// </summary>
    Unsupported = 0,

    /// <summary>
    /// 伝区11: 会社基本情報
    /// </summary>
    CompanyBasic11 = 11,

    /// <summary>
    /// 伝区12: 会社住所
    /// </summary>
    CompanyAddress12 = 12,

    /// <summary>
    /// 伝区13: 会社処理情報
    /// </summary>
    CompanyProcessing13 = 13,

    /// <summary>
    /// 伝区14: 会社手数料
    /// </summary>
    CompanyFee14 = 14,

    /// <summary>
    /// 伝区15: 会社振替情報
    /// </summary>
    CompanyTransfer15 = 15,

    /// <summary>
    /// 伝区16-19: 会社種別
    /// </summary>
    CompanyType16To19 = 16,

    /// <summary>
    /// 伝区71-CBAN1: 会社漢字名
    /// </summary>
    CompanyKanjiName71 = 71,

    /// <summary>
    /// 伝区71-CBAN2: 会社漢字住所
    /// </summary>
    CompanyKanjiAddress71 = 72,

    /// <summary>
    /// 伝区71-CBAN3: 会社漢字連絡先
    /// </summary>
    CompanyKanjiContact71 = 73
}
