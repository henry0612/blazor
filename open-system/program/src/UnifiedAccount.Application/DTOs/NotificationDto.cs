namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 通知DTO
/// </summary>
public record NotificationDto
{
    /// <summary>
    /// IDを取得または設定する。
    /// </summary>
    public long Id { get; init; }
    public string CompanyCode { get; init; } = string.Empty;
    public string AvisCode { get; init; } = string.Empty;
    /// <summary>
    /// 見出しを取得または設定する。
    /// </summary>
    public string? Heading { get; init; }
    /// <summary>
    /// 本文を取得または設定する。
    /// </summary>
    public string? Body { get; init; }
    /// <summary>
    /// 有効開始を取得または設定する。
    /// </summary>
    public DateOnly? ValidFrom { get; init; }
    /// <summary>
    /// 有効終了を取得または設定する。
    /// </summary>
    public DateOnly? ValidTo { get; init; }
}


