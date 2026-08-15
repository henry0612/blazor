namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 金額データDTO
/// </summary>
public record TransferAmountDto
{
    /// <summary>
    /// IDを取得または設定する。
    /// </summary>
    public long Id { get; init; }
    /// <summary>
    /// 契約IDを取得または設定する。
    /// </summary>
    public long? ContractId { get; init; }
    public string RecordType { get; init; } = string.Empty;
    public string BatchNo { get; init; } = string.Empty;
    public int SequenceNo { get; init; }
    public string CompanyCode { get; init; } = string.Empty;
    public string PersonalCode { get; init; } = string.Empty;
    public string CheckDigit { get; init; } = string.Empty;
    /// <summary>
    /// 金額1を取得または設定する。
    /// </summary>
    public decimal? Amount1 { get; init; }
    /// <summary>
    /// 金額2を取得または設定する。
    /// </summary>
    public decimal? Amount2 { get; init; }
    /// <summary>
    /// 金額3を取得または設定する。
    /// </summary>
    public decimal? Amount3 { get; init; }
    /// <summary>
    /// 金額4を取得または設定する。
    /// </summary>
    public decimal? Amount4 { get; init; }
    /// <summary>
    /// 金額5を取得または設定する。
    /// </summary>
    public decimal? Amount5 { get; init; }
    public DateTime UpdatedAt { get; init; }
}
