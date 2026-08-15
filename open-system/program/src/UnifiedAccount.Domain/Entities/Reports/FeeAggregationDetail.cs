using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 手数料集計明細
/// </summary>
[Table("TR_FeeAggregationDetails")]
public class FeeAggregationDetail : BaseEntity
{
    /// <summary>
    /// 手数料集計IDを取得または設定する。
    /// </summary>
    public long FeeAggregationId { get; set; }
    public FeeAggregation FeeAggregation { get; set; } = null!;

    /// <summary>
    /// 行番号を取得または設定する。
    /// </summary>
    public short LineNo { get; set; }

    [MaxLength(4)]
    /// <summary>
    /// 銀行コードを取得または設定する。
    /// </summary>
    public string? BankCode { get; set; }

    /// <summary>
    /// 振替件数を取得または設定する。
    /// </summary>
    public int TransferCount { get; set; }
    [Precision(12, 0)] public decimal TransferAmount { get; set; }
    [Precision(10, 0)] public decimal FeeAmount { get; set; }
}


