namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// 銀行支店マスタ一覧行
/// </summary>
public record BankBranchRow(
    long Id,
    string BankCode,
    string BranchCode,
    string BankNameKana,
    string BranchNameKana,
    string? BankNameKanji,
    string? BranchNameKanji,
    DateTime UpdatedAt);
