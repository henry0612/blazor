using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// UCV請求明細 (KOZXUM+KOZXUN)
/// </summary>
[Table("TR_UcvBillingDetails")]
[Index(nameof(CompanyCode), nameof(BillingYearMonth))]
public class UcvBillingDetail : BaseEntity
{
    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>
    /// 請求年月を取得または設定する。
    /// </summary>
    public DateOnly BillingYearMonth { get; set; }

    [Required, MaxLength(4)]
    public string ItemCode { get; set; } = string.Empty;

    [Precision(10, 2)]
    /// <summary>
    /// 単位単価を取得または設定する。
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// 数量を取得または設定する。
    /// </summary>
    public int Quantity { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 合計金額を取得または設定する。
    /// </summary>
    public decimal TotalAmount { get; set; }

    [Precision(5, 2)]
    /// <summary>
    /// 税率を取得または設定する。
    /// </summary>
    public decimal? TaxRate { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 税金額を取得または設定する。
    /// </summary>
    public decimal? TaxAmount { get; set; }

    /// <summary>
    /// 請求書フラグを取得または設定する。
    /// </summary>
    public bool InvoiceFlag { get; set; }
}


