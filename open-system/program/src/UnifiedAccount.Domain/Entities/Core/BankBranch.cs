using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Domain.Entities.Changes;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 金融機関支店マスター (BNKXBM)
/// </summary>
[Table("TM_BankBranches")]
[Index(nameof(BankCode), nameof(BranchCode), IsUnique = true)]
public class BankBranch : BaseEntity
{
    [Required, MaxLength(4)]
    public string BankCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string BranchCode { get; set; } = string.Empty;

    [Required, MaxLength(15)]
    public string BankNameKana { get; set; } = string.Empty;

    [Required, MaxLength(15)]
    public string BranchNameKana { get; set; } = string.Empty;

    [MaxLength(30)]
    /// <summary>
    /// 事務所名称を取得または設定する。
    /// </summary>
    public string? OfficeName { get; set; }

    [MaxLength(15)]
    /// <summary>
    /// 銀行名称漢字を取得または設定する。
    /// </summary>
    public string? BankNameKanji { get; set; }

    [MaxLength(15)]
    /// <summary>
    /// 支店名称漢字を取得または設定する。
    /// </summary>
    public string? BranchNameKanji { get; set; }

    [Required, MaxLength(1)]
    public string KanjiSetFlag { get; set; } = "0";

    // Navigation
    public ICollection<Contract> Contracts { get; set; } = [];
    public ICollection<BankBranchChange> BankBranchChanges { get; set; } = [];
}


