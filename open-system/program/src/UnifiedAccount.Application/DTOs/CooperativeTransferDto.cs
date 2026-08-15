namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 共済連振替DTO
/// </summary>
public record CooperativeTransferDto
{
    /// <summary>
    /// IDを取得または設定する。
    /// </summary>
    public long Id { get; init; }
    /// <summary>
    /// 契約番号を取得または設定する。
    /// </summary>
    public string? ContractNo { get; init; }
    /// <summary>
    /// 会員コードを取得または設定する。
    /// </summary>
    public string? MemberCode { get; init; }
    /// <summary>
    /// プランコードを取得または設定する。
    /// </summary>
    public string? PlanCode { get; init; }
    /// <summary>
    /// 金額を取得または設定する。
    /// </summary>
    public decimal? Amount { get; init; }
    /// <summary>
    /// 結果コードを取得または設定する。
    /// </summary>
    public string? ResultCode { get; init; }
}


