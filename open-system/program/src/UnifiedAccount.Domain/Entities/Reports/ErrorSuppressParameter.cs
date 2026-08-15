using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// トータルエラー抑止パラメータ (KUBUN='M'相当)
/// </summary>
[Table("TM_ErrorSuppressParameters")]
[Index(nameof(CompanyCode), IsUnique = true)]
public class ErrorSuppressParameter : BaseEntity
{
    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required]
    public bool SuppressFlag { get; set; } = true;

    [MaxLength(200)]
    /// <summary>
    /// 説明を取得または設定する。
    /// </summary>
    public string? Description { get; set; }
}


