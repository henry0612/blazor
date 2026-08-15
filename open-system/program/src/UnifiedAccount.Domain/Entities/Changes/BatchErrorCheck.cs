using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Changes;

/// <summary>
/// バッチエラーチェック (KOZXBN)
/// </summary>
[Table("TD_BatchErrorChecks")]
[Index(nameof(CompanyCode), nameof(BatchNo), IsUnique = true)]
public class BatchErrorCheck : BaseEntity
{
    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string BatchNo { get; set; } = string.Empty;

    [Required, MaxLength(1)] public string ErrorFlag1 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag2 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag3 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag4 { get; set; } = "0";
    [Required, MaxLength(1)] public string ErrorFlag5 { get; set; } = "0";

    [Precision(10, 0)] public decimal Total1 { get; set; }
    [Precision(10, 0)] public decimal Total2 { get; set; }
    [Precision(10, 0)] public decimal Total3 { get; set; }
    [Precision(10, 0)] public decimal Total4 { get; set; }
    [Precision(10, 0)] public decimal Total5 { get; set; }
}

