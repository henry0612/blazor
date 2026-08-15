using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 銀行本店設定 (KOZXBH)
/// </summary>
[Table("TM_BankHeadOffices")]
[Index(nameof(BankCode), IsUnique = true)]
public class BankHeadOffice : BaseEntity
{
    [Required, MaxLength(4)]
    public string BankCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string HeadBranchCode { get; set; } = string.Empty;

    [MaxLength(15)]
    /// <summary>
    /// 銀行名称を取得または設定する。
    /// </summary>
    public string? BankName { get; set; }
}


