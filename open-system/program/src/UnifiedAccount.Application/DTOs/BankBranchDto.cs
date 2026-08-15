namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 銀行支店DTO
/// </summary>
public record BankBranchDto
{
    /// <summary>
    /// IDを取得または設定する。
    /// </summary>
    public long Id { get; init; }
    public string BankCode { get; init; } = string.Empty;
    public string BranchCode { get; init; } = string.Empty;
    public string BankNameKana { get; init; } = string.Empty;
    public string BranchNameKana { get; init; } = string.Empty;
    /// <summary>
    /// 銀行名称漢字を取得または設定する。
    /// </summary>
    public string? BankNameKanji { get; init; }
    /// <summary>
    /// 支店名称漢字を取得または設定する。
    /// </summary>
    public string? BranchNameKanji { get; init; }
    /// <summary>
    /// 楽観ロック用の最終更新日時
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}


