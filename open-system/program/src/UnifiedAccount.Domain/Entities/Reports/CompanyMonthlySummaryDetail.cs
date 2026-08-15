using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 会社別月次集計明細
/// </summary>
[Table("TR_CompanyMonthlySummaryDetails")]
public class CompanyMonthlySummaryDetail : BaseEntity
{
    /// <summary>
    /// 会社月次集計IDを取得または設定する。
    /// </summary>
    public long CompanyMonthlySummaryId { get; set; }
    public CompanyMonthlySummary CompanyMonthlySummary { get; set; } = null!;

    /// <summary>
    /// 月番号を取得または設定する。
    /// </summary>
    public short MonthNo { get; set; }

    [Precision(12, 0)] public decimal TransferAmount { get; set; }
    [Precision(12, 0)] public decimal CollectionAmount { get; set; }
    /// <summary>
    /// 振替件数を取得または設定する。
    /// </summary>
    public int TransferCount { get; set; }
    /// <summary>
    /// 収納件数を取得または設定する。
    /// </summary>
    public int CollectionCount { get; set; }
}


