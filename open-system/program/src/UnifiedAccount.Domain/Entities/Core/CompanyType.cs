using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 会社種別 (CM-SHUBETSU OCCURS 4)
/// </summary>
[Table("TM_CompanyTypes")]
public class CompanyType : BaseEntity
{
    /// <summary>
    /// 会社IDを取得または設定する。
    /// </summary>
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    /// <summary>
    /// 種別番号を取得または設定する。
    /// </summary>
    public short TypeNo { get; set; }

    [Required, MaxLength(1)]
    public string TypeCode { get; set; } = string.Empty;

    [MaxLength(2)]
    /// <summary>
    /// 周期を取得または設定する。
    /// </summary>
    public string? Cycle { get; set; }

    [MaxLength(6)]
    /// <summary>
    /// 開始年月を取得または設定する。
    /// </summary>
    public string? StartYearMonth { get; set; }

    [MaxLength(20)]
    /// <summary>
    /// 種別名称を取得または設定する。
    /// </summary>
    public string? TypeName { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 金額を取得または設定する。
    /// </summary>
    public decimal Amount { get; set; }
}


