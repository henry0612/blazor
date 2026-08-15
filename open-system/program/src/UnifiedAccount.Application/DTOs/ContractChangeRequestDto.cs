namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 契約異動DTO
/// </summary>
public record ContractChangeRequestDto
{
    /// <summary>
    /// IDを取得または設定する。
    /// </summary>
    public long Id { get; init; }
    public string CompanyCode { get; init; } = string.Empty;
    public string PersonalCode { get; init; } = string.Empty;
    public string RequestType { get; init; } = string.Empty;
    /// <summary>
    /// 変更処理内容を取得または設定する。
    /// </summary>
    public string? ChangeAction { get; init; }
}
