namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 契約者DTO
/// </summary>
public record ContractDto
{
    /// <summary>
    /// IDを取得または設定する。
    /// </summary>
    public long Id { get; init; }
    public string CompanyCode { get; init; } = string.Empty;
    public string PersonalCode { get; init; } = string.Empty;
    public string CheckDigit { get; init; } = string.Empty;
    public string DepositorNameKana { get; init; } = string.Empty;
    /// <summary>
    /// 預金者名称漢字を取得または設定する。
    /// </summary>
    public string? DepositorNameKanji { get; init; }
    public string BankCode { get; init; } = string.Empty;
    public string BranchCode { get; init; } = string.Empty;
    public string AccountType { get; init; } = string.Empty;
    public string AccountNo { get; init; } = string.Empty;
    /// <summary>
    /// 引落日を取得または設定する。
    /// </summary>
    public short WithdrawalDay { get; init; }
    /// <summary>
    /// 当回請求金額を取得または設定する。
    /// </summary>
    public decimal CurrentBillingAmount { get; init; }
    public string SuspendFlag { get; init; } = "0";
}


