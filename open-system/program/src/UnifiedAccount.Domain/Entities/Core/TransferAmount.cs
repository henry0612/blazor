using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 金額データ (KOZXKD)
/// </summary>
[Table("TD_TransferAmounts")]
[Index(nameof(CompanyCode), nameof(PersonalCode), nameof(CheckDigit))]
[Index(nameof(CompanyCode), nameof(BatchNo), nameof(SequenceNo))]
public class TransferAmount : BaseEntity
{
    /// <summary>
    /// 契約IDを取得または設定する。
    /// </summary>
    public long? ContractId { get; set; }
    /// <summary>
    /// 契約を取得または設定する。
    /// </summary>
    public Contract? Contract { get; set; }

    [Required, MaxLength(2)]
    public string RecordType { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string BatchNo { get; set; } = string.Empty;

    [Required]
    public int SequenceNo { get; set; }

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PersonalCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string CheckDigit { get; set; } = string.Empty;

    [Precision(10, 0)] public decimal? Amount1 { get; set; }
    [Precision(10, 0)] public decimal? Amount2 { get; set; }
    [Precision(10, 0)] public decimal? Amount3 { get; set; }
    [Precision(10, 0)] public decimal? Amount4 { get; set; }
    [Precision(10, 0)] public decimal? Amount5 { get; set; }

    [Required, MaxLength(1)] public string ErrorFlag1 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag2 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag3 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag4 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag5 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag6 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag7 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag8 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag9 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag10 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag11 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag12 { get; set; } = "0";
}
