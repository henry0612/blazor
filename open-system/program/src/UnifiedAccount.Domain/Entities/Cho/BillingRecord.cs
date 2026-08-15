using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Cho;

/// <summary>
/// 請求実績 (CHOXSR)
/// </summary>
[Table("TD_BillingRecords", Schema = "cho")]
[Index(nameof(SettlementType), nameof(BusinessCode), nameof(ItemCode), nameof(SalesYearMonth), IsUnique = true)]
public class BillingRecord : BaseEntity
{
    [Required, MaxLength(2)]
    public string SettlementType { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string BusinessCode { get; set; } = string.Empty;

    [Required, MaxLength(4)]
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// 売上年月を取得または設定する。
    /// </summary>
    public DateOnly SalesYearMonth { get; set; }

    [Precision(10, 2)]
    /// <summary>
    /// 単位単価を取得または設定する。
    /// </summary>
    public decimal UnitPrice { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 数量を取得または設定する。
    /// </summary>
    public decimal Quantity { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 合計金額を取得または設定する。
    /// </summary>
    public decimal TotalAmount { get; set; }

    [MaxLength(1)]
    /// <summary>
    /// 業務処理種別を取得または設定する。
    /// </summary>
    public string? BusinessProcessType { get; set; }

    /// <summary>
    /// 業務処理日付を取得または設定する。
    /// </summary>
    public DateOnly? BusinessProcessDate { get; set; }

    [MaxLength(1)]
    /// <summary>
    /// 経理結果種別を取得または設定する。
    /// </summary>
    public string? AccountingResultType { get; set; }

    /// <summary>
    /// 経理処理日付を取得または設定する。
    /// </summary>
    public DateOnly? AccountingProcessDate { get; set; }
}


