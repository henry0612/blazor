using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Cho;

/// <summary>
/// CHO所有の全銀仕様バッチヘッダ
/// </summary>
[Table("TD_ChoZenginBatches", Schema = "cho")]
[Index(nameof(WithdrawalDate))]
[Index(nameof(ConsignorCode))]
public class ChoZenginBatch : BaseEntity
{
    [Required, MaxLength(2)]
    public string TypeCode { get; set; } = "91";

    [Required, MaxLength(10)]
    public string ConsignorCode { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string ConsignorName { get; set; } = string.Empty;

    public DateOnly WithdrawalDate { get; set; }

    [MaxLength(4)]
    public string? TransferBankCode { get; set; }

    [MaxLength(3)]
    public string? TransferBranchCode { get; set; }

    [MaxLength(10)]
    public string? TransferAccountNo { get; set; }

    public int? TotalCount { get; set; }

    [Precision(12, 0)]
    public decimal? TotalAmount { get; set; }

    public int? SettledCount { get; set; }

    [Precision(12, 0)]
    public decimal? SettledAmount { get; set; }

    public int? FailedCount { get; set; }

    [Precision(12, 0)]
    public decimal? FailedAmount { get; set; }

    public ICollection<ChoZenginTransaction> Transactions { get; set; } = [];
}
