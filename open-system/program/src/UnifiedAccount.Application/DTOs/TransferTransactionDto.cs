namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 振替トランザクションDTO
/// </summary>
public record TransferTransactionDto
{
    /// <summary>
    /// IDを取得または設定する。
    /// </summary>
    public long Id { get; init; }
    public string ConsignorCode { get; init; } = string.Empty;
    /// <summary>
    /// 引落日付を取得または設定する。
    /// </summary>
    public DateOnly WithdrawalDate { get; init; }
    public string CompanyCode { get; init; } = string.Empty;
    public string PersonalCode { get; init; } = string.Empty;
    /// <summary>
    /// 金額を取得または設定する。
    /// </summary>
    public decimal Amount { get; init; }
    /// <summary>
    /// 結果コードを取得または設定する。
    /// </summary>
    public string? ResultCode { get; init; }
}


