namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 全銀明細DTO
/// </summary>
public record ZenginTransactionDto
{
    /// <summary>
    /// IDを取得または設定する。
    /// </summary>
    public long Id { get; init; }
    public string BankCode { get; init; } = string.Empty;
    public string BranchCode { get; init; } = string.Empty;
    public string AccountNo { get; init; } = string.Empty;
    public string DepositorName { get; init; } = string.Empty;
    /// <summary>
    /// 金額を取得または設定する。
    /// </summary>
    public decimal Amount { get; init; }
    /// <summary>
    /// 結果コードを取得または設定する。
    /// </summary>
    public string? ResultCode { get; init; }
}


