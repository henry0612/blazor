using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Domain.Entities.Changes;

/// <summary>
/// 金額修正 (KOZXKX)
/// </summary>
[Table("TD_AmountModifications")]
[Index(nameof(CompanyCode), nameof(BatchNo), nameof(SequenceNo), IsUnique = true)]
public class AmountModification : BaseEntity
{
    /// <summary>
    /// 会社IDを取得または設定する。
    /// </summary>
    public long? CompanyId { get; set; }
    /// <summary>
    /// 会社を取得または設定する。
    /// </summary>
    public Company? Company { get; set; }

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string BatchNo { get; set; } = string.Empty;

    [Required, MaxLength(7)]
    public string SequenceNo { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    public string TransferType { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string KdBatchNo { get; set; } = string.Empty;

    [Required, MaxLength(6)]
    public string ContractCompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string ContractPersonalCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string ContractCheckDigit { get; set; } = string.Empty;

    [Precision(10, 0)] public decimal Amount1 { get; set; }
    [Precision(10, 0)] public decimal Amount2 { get; set; }
    [Precision(10, 0)] public decimal Amount3 { get; set; }
    [Precision(10, 0)] public decimal Amount4 { get; set; }
    [Precision(10, 0)] public decimal Amount5 { get; set; }

    [Required, MaxLength(12)]
    public string ErrorFlags { get; set; } = "000000000000";
}


