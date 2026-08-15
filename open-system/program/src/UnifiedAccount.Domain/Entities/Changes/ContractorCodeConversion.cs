using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Changes;

/// <summary>
/// コード変換 (KOZXCV)
/// </summary>
[Table("TD_ContractorCodeConversions")]
[Index(nameof(OldCompanyCode), nameof(OldPersonalCode))]
public class ContractorCodeConversion : BaseEntity
{
    [Required, MaxLength(6)]
    public string OldCompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string OldPersonalCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string NewPersonalCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string ErrorFlag { get; set; } = "0";
}

