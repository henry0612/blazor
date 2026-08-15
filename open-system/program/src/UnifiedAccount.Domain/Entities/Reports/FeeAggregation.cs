using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 手数料集計 (KOZXTS)
/// </summary>
[Table("TR_FeeAggregations")]
public class FeeAggregation : BaseEntity
{
    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>
    /// 処理日付を取得または設定する。
    /// </summary>
    public DateOnly ProcessingDate { get; set; }

    // Navigation
    public ICollection<FeeAggregationDetail> Details { get; set; } = [];
}


