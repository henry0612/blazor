using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 口座リンクデータ (KOZXND)
/// </summary>
[Table("TR_AccountLinks")]
[Index(nameof(CompanyCode), nameof(PersonalCode))]
public class AccountLink : BaseEntity
{
    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PersonalCode { get; set; } = string.Empty;

    [Required, MaxLength(4)]
    public string LinkedBankCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string LinkedBranchCode { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string LinkedAccountNo { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string LinkType { get; set; } = string.Empty;
}

