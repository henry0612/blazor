using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 契約者削除ログ (TR_ContractMasterDeleteLogs)
/// </summary>
[Table("TR_ContractMasterDeleteLogs")]
[Index(nameof(JobExecutionId), nameof(CompanyCode), nameof(PersonalCode), IsUnique = true)]
[Index(nameof(JobExecutionId))]
public class ContractMasterDeleteLog : BaseEntity
{
    [Required, MaxLength(50)]
    public string JobExecutionId { get; set; } = string.Empty;

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PersonalCode { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string DepositorName { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string ContractorName { get; set; } = string.Empty;

    [Required, MaxLength(4)]
    public string BankCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string BranchCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string AccountType { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    public DateOnly DeletionDate { get; set; }
}
