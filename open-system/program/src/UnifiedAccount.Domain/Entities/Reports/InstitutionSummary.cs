using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 金融機関総括表 (KOZXSK)
/// </summary>
[Table("TR_InstitutionSummaries")]
[Index(nameof(BankCode), nameof(BranchCode))]
public class InstitutionSummary : BaseEntity
{
    [Required, MaxLength(4)]
    public string BankCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string BranchCode { get; set; } = string.Empty;

    [MaxLength(15)] public string? BankName { get; set; }
    [MaxLength(15)] public string? BranchName { get; set; }

    /// <summary>
    /// 合計件数を取得または設定する。
    /// </summary>
    public int TotalCount { get; set; }
    [Precision(12, 0)] public decimal TotalAmount { get; set; }
    /// <summary>
    /// 決済済件数を取得または設定する。
    /// </summary>
    public int SettledCount { get; set; }
    [Precision(12, 0)] public decimal SettledAmount { get; set; }
    /// <summary>
    /// 失敗件数を取得または設定する。
    /// </summary>
    public int FailedCount { get; set; }
    [Precision(12, 0)] public decimal FailedAmount { get; set; }

    /// <summary>
    /// 処理日付を取得または設定する。
    /// </summary>
    public DateOnly ProcessDate { get; set; }
}


