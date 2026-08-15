using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 会社別月次集計 (KOZXSF)
/// </summary>
[Table("TR_CompanyMonthlySummaries")]
[Index(nameof(CompanyCode), nameof(FiscalYear), IsUnique = true)]
public class CompanyMonthlySummary : BaseEntity
{
    /// <summary>
    /// 会社IDを取得または設定する。
    /// </summary>
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>
    /// 会計年度年を取得または設定する。
    /// </summary>
    public short FiscalYear { get; set; }

    // Navigation
    public ICollection<CompanyMonthlySummaryDetail> Details { get; set; } = [];
}


