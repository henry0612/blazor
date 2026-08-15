using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 管理会計 (KOZXKZ)
/// </summary>
[Table("TR_ManagementAccountings")]
public class ManagementAccounting : BaseEntity
{
    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>
    /// 経理日付を取得または設定する。
    /// </summary>
    public DateOnly AccountingDate { get; set; }

    [Required, MaxLength(4)]
    public string AccountCode { get; set; } = string.Empty;

    [Precision(12, 0)]
    /// <summary>
    /// 金額を取得または設定する。
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 件数を取得または設定する。
    /// </summary>
    public int Count { get; set; }
}


