using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 通知メッセージ (KOZXMS)
/// </summary>
[Table("TD_NotificationMessages")]
[Index(nameof(CompanyCode), nameof(AvisCode), IsUnique = true)]
public class NotificationMessage : BaseEntity
{
    /// <summary>
    /// 会社IDを取得または設定する。
    /// </summary>
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    public string AvisCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string ApplicableType { get; set; } = string.Empty;

    /// <summary>
    /// 有効開始を取得または設定する。
    /// </summary>
    public DateOnly? ValidFrom { get; set; }
    /// <summary>
    /// 有効終了を取得または設定する。
    /// </summary>
    public DateOnly? ValidTo { get; set; }

    /// <summary>
    /// 見出しを取得または設定する。
    /// </summary>
    public string? Heading { get; set; }
    /// <summary>
    /// 本文を取得または設定する。
    /// </summary>
    public string? Body { get; set; }
}


