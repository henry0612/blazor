namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 会社マスタDTO
/// </summary>
public record CompanyDto
{
    /// <summary>
    /// IDを取得または設定する。
    /// </summary>
    public long Id { get; init; }
    public string CompanyCode { get; init; } = string.Empty;
    public string CompanyNameKana { get; init; } = string.Empty;
    /// <summary>
    /// 会社名称漢字を取得または設定する。
    /// </summary>
    public string? CompanyNameKanji { get; init; }
    public string ConsignorCode { get; init; } = string.Empty;
    /// <summary>
    /// 郵便コードを取得または設定する。
    /// </summary>
    public string? PostalCode { get; init; }
    /// <summary>
    /// 都道府県を取得または設定する。
    /// </summary>
    public string? Prefecture { get; init; }
    /// <summary>
    /// 電話番号を取得または設定する。
    /// </summary>
    public string? PhoneNumber { get; init; }
    /// <summary>
    /// 基本手数料を取得または設定する。
    /// </summary>
    public decimal BasicFee { get; init; }
    /// <summary>
    /// 管理手数料を取得または設定する。
    /// </summary>
    public decimal AdminFee { get; init; }
}


