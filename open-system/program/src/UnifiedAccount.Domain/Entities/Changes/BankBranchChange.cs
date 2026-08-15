using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Domain.Entities.Changes;

/// <summary>
/// 金融機関異動 (BNKXDT)
/// </summary>
[Table("TD_BankBranchChanges")]
[Index(nameof(BankCode), nameof(BranchCode))]
public class BankBranchChange : BaseEntity
{
    /// <summary>
    /// 銀行支店IDを取得または設定する。
    /// </summary>
    public long? BankBranchId { get; set; }
    /// <summary>
    /// 銀行支店を取得または設定する。
    /// </summary>
    public BankBranch? BankBranch { get; set; }

    [Required, MaxLength(4)]
    public string BankCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string BranchCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string ChangeType { get; set; } = string.Empty;

    [MaxLength(15)] public string? BankNameKana { get; set; }
    [MaxLength(15)] public string? BranchNameKana { get; set; }
    [MaxLength(15)] public string? BankNameKanji { get; set; }
    [MaxLength(15)] public string? BranchNameKanji { get; set; }
}


