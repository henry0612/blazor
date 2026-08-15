using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Cho;

/// <summary>
/// CHO所有の全銀仕様明細
/// </summary>
[Table("TD_ChoZenginTransactions", Schema = "cho")]
[Index(nameof(ZenginBatchId))]
public class ChoZenginTransaction : BaseEntity
{
    public long ZenginBatchId { get; set; }

    public ChoZenginBatch ZenginBatch { get; set; } = null!;

    [Required, MaxLength(4)]
    public string BankCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string BranchCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string AccountType { get; set; } = string.Empty;

    [Required, MaxLength(7)]
    public string AccountNo { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string DepositorName { get; set; } = string.Empty;

    [Precision(10, 0)]
    public decimal Amount { get; set; }

    [Required, MaxLength(1)]
    public string NewCode { get; set; } = "0";

    [MaxLength(19)]
    public string? ContractorCode { get; set; }

    [MaxLength(1)]
    public string? ResultCode { get; set; }
}
