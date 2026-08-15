using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// AuditRecord を表す。
/// </summary>
[Table("TD_AuditRecords")]
[Index(nameof(ActorId), nameof(OccurredAt))]
[Index(nameof(FeatureId), nameof(OccurredAt))]
[Index(nameof(TargetType), nameof(TargetKey), nameof(OccurredAt))]
public class AuditRecord : BaseEntity
{
    /// <summary>
    /// OccurredAt を取得または設定する。
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>
    /// ActorId を取得または設定する。
    /// </summary>
    [Required, MaxLength(100)]
    public string ActorId { get; set; } = string.Empty;

    /// <summary>
    /// FeatureId を取得または設定する。
    /// </summary>
    [Required, MaxLength(20)]
    public string FeatureId { get; set; } = string.Empty;

    /// <summary>
    /// Action を取得または設定する。
    /// </summary>
    [Required, MaxLength(20)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// TargetType を取得または設定する。
    /// </summary>
    [Required, MaxLength(100)]
    public string TargetType { get; set; } = string.Empty;

    /// <summary>
    /// TargetKey を取得または設定する。
    /// </summary>
    [Required, MaxLength(1000)]
    public string TargetKey { get; set; } = string.Empty;

    /// <summary>
    /// Result を取得または設定する。
    /// </summary>
    [Required, MaxLength(32)]
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// CorrelationId を取得または設定する。
    /// </summary>
    [Required, MaxLength(100)]
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// BeforeValuesJson を取得または設定する。
    /// </summary>
    [Required]
    public string BeforeValuesJson { get; set; } = "{}";

    /// <summary>
    /// AfterValuesJson を取得または設定する。
    /// </summary>
    [Required]
    public string AfterValuesJson { get; set; } = "{}";

    /// <summary>
    /// ErrorCode を取得または設定する。
    /// </summary>
    [MaxLength(50)]
    public string? ErrorCode { get; set; }
}
