namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 振替不能DTO
/// </summary>
public record TransferFailureDto
{
    /// <summary>
    /// IDを取得または設定する。
    /// </summary>
    public long Id { get; init; }
    /// <summary>
    /// 契約IDを取得または設定する。
    /// </summary>
    public long? ContractId { get; init; }
    public string CompanyCode { get; init; } = string.Empty;
    public string PersonalCode { get; init; } = string.Empty;
    public string TransferType { get; init; } = string.Empty;
    public string ResultCode { get; init; } = string.Empty;
    public string BankCode { get; init; } = string.Empty;
}


