using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 伝送パラメータ (KOZXDI)
/// </summary>
[Table("TM_TransmissionParameters")]
[Index(nameof(ParameterKey), IsUnique = true)]
public class TransmissionParameter : BaseEntity
{
    [Required, MaxLength(50)]
    public string ParameterKey { get; set; } = string.Empty;

    [MaxLength(200)]
    /// <summary>
    /// パラメータ値を取得または設定する。
    /// </summary>
    public string? ParameterValue { get; set; }

    [MaxLength(100)]
    /// <summary>
    /// 説明を取得または設定する。
    /// </summary>
    public string? Description { get; set; }
}


