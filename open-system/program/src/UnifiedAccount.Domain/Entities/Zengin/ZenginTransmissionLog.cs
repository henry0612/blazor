using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Zengin;

/// <summary>
/// 全銀送受信ログ (KOZXZK)
/// </summary>
[Table("TD_ZenginTransmissionLogs", Schema = "zengin")]
[Index(nameof(ManagementDate))]
public class ZenginTransmissionLog : BaseEntity
{
    /// <summary>
    /// 全銀バッチIDを取得または設定する。
    /// </summary>
    public long? ZenginBatchId { get; set; }
    /// <summary>
    /// 全銀バッチを取得または設定する。
    /// </summary>
    public ZenginBatch? ZenginBatch { get; set; }

    /// <summary>
    /// 管理日付を取得または設定する。
    /// </summary>
    public DateOnly ManagementDate { get; set; }
    /// <summary>
    /// 管理時刻を取得または設定する。
    /// </summary>
    public TimeOnly ManagementTime { get; set; }

    [Required, MaxLength(2)]
    public string WithdrawalMonth { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    public string WithdrawalDay { get; set; } = string.Empty;

    /// <summary>
    /// レコード件数を取得または設定する。
    /// </summary>
    public int RecordCount { get; set; }

    /// <summary>
    /// 配信日付を取得または設定する。
    /// </summary>
    public DateOnly? DeliveryDate { get; set; }
    /// <summary>
    /// 配信時刻を取得または設定する。
    /// </summary>
    public TimeOnly? DeliveryTime { get; set; }

    [Required, MaxLength(1)]
    public string DeliveryFlag { get; set; } = "0";

    /// <summary>
    /// 受信反映フラグ (KOZFPBKR CTRL-FG相当)
    /// "0"=未受信, "1"=受信済 (JOB3専用)
    /// </summary>
    [MaxLength(1)]
    public string ResultReceiveFlag { get; set; } = "0";

    /// <summary>
    /// 受信反映日 (JOB3結果受信完了日)
    /// </summary>
    public DateOnly? ResultReceiveDate { get; set; }
}


