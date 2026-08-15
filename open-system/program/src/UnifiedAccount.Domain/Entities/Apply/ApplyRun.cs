using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Apply;

/// <summary>
/// 適用試行 (新規)
/// </summary>
/// <remarks>
/// 振替実行バッチの適用フェーズ1回分に対応する。
/// JOB2 では担当者の確認を挟んで複数回実行されるため、AttemptNo で識別する。
/// </remarks>
[Table("TD_ApplyRuns")]
[Index(nameof(RunJob), nameof(TransferDate), nameof(AttemptNo), IsUnique = true)]
public class ApplyRun : BaseEntity
{
    /// <summary>
    /// ジョブ実行IDを取得または設定する。
    /// </summary>
    [Required, MaxLength(50)]
    public string JobExecutionId { get; set; } = string.Empty;

    /// <summary>
    /// ジョブ系統 (JOB1 / JOB2 / JOB3) を取得または設定する。
    /// </summary>
    [Required, MaxLength(8)]
    public string RunJob { get; set; } = string.Empty;

    /// <summary>
    /// 振替日を取得または設定する。
    /// </summary>
    public DateOnly TransferDate { get; set; }

    /// <summary>
    /// 試行番号を取得または設定する。同一 RunJob + TransferDate 内で 1 起点。
    /// </summary>
    public short AttemptNo { get; set; }

    /// <summary>
    /// 状態を取得または設定する。
    /// RUNNING / REVIEWING / CONFIRMED / ABANDONED / FAILED のいずれか。
    /// </summary>
    [Required, MaxLength(16)]
    public string Status { get; set; } = "RUNNING";

    /// <summary>
    /// 開始日時を取得または設定する。
    /// </summary>
    public DateTime StartedAt { get; set; } = LocalDateTimeProvider.Now;

    /// <summary>
    /// 適用フェーズの完了日時を取得または設定する。
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 実行者を取得または設定する。
    /// </summary>
    [Required, MaxLength(50)]
    public string ExecutedBy { get; set; } = string.Empty;

    /// <summary>
    /// 契約者マスタへの反映件数を取得または設定する。
    /// </summary>
    public int ContractApplyCount { get; set; }

    /// <summary>
    /// 契約者マスタ反映のエラー件数を取得または設定する。
    /// </summary>
    public int ContractErrorCount { get; set; }

    /// <summary>
    /// 金額データへの反映件数を取得または設定する。
    /// </summary>
    public int AmountApplyCount { get; set; }

    /// <summary>
    /// 金額データ反映のエラー件数を取得または設定する。
    /// </summary>
    public int AmountErrorCount { get; set; }

    /// <summary>
    /// 承認者を取得または設定する。確定フェーズの起動パラメータで受け取る。
    /// </summary>
    [MaxLength(50)]
    public string? ConfirmedBy { get; set; }

    /// <summary>
    /// 承認日時を取得または設定する。
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }
}
