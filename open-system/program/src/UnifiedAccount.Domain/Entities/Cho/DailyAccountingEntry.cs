using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Cho;

/// <summary>
/// 日計仕訳データ (CHOXTD)
/// </summary>
[Table("TD_DailyAccountingEntries", Schema = "cho")]
[Index(nameof(SettlementDate))]
[Index(nameof(CooperativeNo), nameof(BranchOfficeNo))]
public class DailyAccountingEntry : BaseEntity
{
    [Required, MaxLength(2)]
    public string DataType { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string CooperativeNo { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    public string BranchOfficeNo { get; set; } = string.Empty;

    [Required, MaxLength(4)]
    public string AccountCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string DebitCreditType { get; set; } = string.Empty;

    [Precision(13, 0)]
    /// <summary>
    /// 取引金額を取得または設定する。
    /// </summary>
    public decimal TransactionAmount { get; set; }

    /// <summary>
    /// 決済日付を取得または設定する。
    /// </summary>
    public DateOnly SettlementDate { get; set; }
    /// <summary>
    /// 振替件数を取得または設定する。
    /// </summary>
    public int TransferCount { get; set; }
    /// <summary>
    /// 処理日付を取得または設定する。
    /// </summary>
    public DateOnly? ProcessingDate { get; set; }

    [MaxLength(5)]
    /// <summary>
    /// 取引コードを取得または設定する。
    /// </summary>
    public string? TransactionCode { get; set; }

    [MaxLength(1)]
    /// <summary>
    /// 小計種別を取得または設定する。
    /// </summary>
    public string? SubtotalType { get; set; }
}


