using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Zengin;

/// <summary>
/// 全銀バッチヘッダ (KOZXZD HEAD)
/// </summary>
[Table("TD_ZenginBatches", Schema = "zengin")]
[Index(nameof(WithdrawalDate))]
[Index(nameof(ConsignorCode))]
public class ZenginBatch : BaseEntity
{
    [Required, MaxLength(2)]
    public string TypeCode { get; set; } = "91";

    [Required, MaxLength(10)]
    public string ConsignorCode { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string ConsignorName { get; set; } = string.Empty;

    /// <summary>
    /// 引落日付を取得または設定する。
    /// </summary>
    public DateOnly WithdrawalDate { get; set; }

    [MaxLength(4)] public string? TransferBankCode { get; set; }
    [MaxLength(3)] public string? TransferBranchCode { get; set; }
    [MaxLength(10)] public string? TransferAccountNo { get; set; }

    /// <summary>
    /// 合計件数を取得または設定する。
    /// </summary>
    public int? TotalCount { get; set; }
    [Precision(12, 0)] public decimal? TotalAmount { get; set; }
    /// <summary>
    /// 決済済件数を取得または設定する。
    /// </summary>
    public int? SettledCount { get; set; }
    [Precision(12, 0)] public decimal? SettledAmount { get; set; }
    /// <summary>
    /// 失敗件数を取得または設定する。
    /// </summary>
    public int? FailedCount { get; set; }
    [Precision(12, 0)] public decimal? FailedAmount { get; set; }

    // Navigation
    public ICollection<ZenginTransaction> Transactions { get; set; } = [];
    public ICollection<ZenginTransmissionLog> TransmissionLogs { get; set; } = [];
}


