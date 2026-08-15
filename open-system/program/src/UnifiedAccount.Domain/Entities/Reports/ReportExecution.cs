using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 帳票実行履歴 (F-INF-004)
/// 実行 ID（帳票1印刷ごとの一意識別子）、ステータス、リトライ回数、出力ファイルパスなどを追跡
/// </summary>
[Table("TR_ReportExecutions")]
[Index(nameof(ExecutionId))]
[Index(nameof(JobExecutionId))]
[Index(nameof(TemplateId))]
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
public class ReportExecution : BaseEntity
{
    /// <summary>
    /// 帳票印刷単位の一意識別子（1印刷 = 1レコード）
    /// </summary>
    [Required, MaxLength(50)]
    public string ExecutionId { get; set; } = string.Empty;

    /// <summary>
    /// ジョブフロー識別子（JobContext.JobExecutionId）。1ジョブ内で複数印刷が発生する場合に同一値を持つ
    /// </summary>
    [MaxLength(100)]
    public string? JobExecutionId { get; set; }

    /// <summary>
    /// テンプレート ID
    /// </summary>
    [Required, MaxLength(50)]
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// 実行ステータス（Pending/Running/Succeeded/PartialOutput/Failed）
    /// </summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// 出力モード（Buffered/Streamed）
    /// </summary>
    [Required, MaxLength(20)]
    public string OutputMode { get; set; } = string.Empty;

    /// <summary>
    /// 部分出力フラグ（Streamed 中断時の .incomplete ファイル）
    /// </summary>
    public bool IsPartialOutput { get; set; }

    /// <summary>
    /// 出力ファイルパス
    /// </summary>
    [MaxLength(500)]
    public string? OutputFilePath { get; set; }

    /// <summary>
    /// 未完了ファイルパス（.incomplete）
    /// </summary>
    [MaxLength(500)]
    public string? IncompleteFilePath { get; set; }

    /// <summary>
    /// リトライ回数
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 最大リトライ回数
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// 実行開始時刻
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 実行終了時刻
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 実行時間（ミリ秒）
    /// </summary>
    public long? ElapsedMilliseconds { get; set; }

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// エラースタックトレース
    /// </summary>
    [MaxLength(4000)]
    public string? ErrorStackTrace { get; set; }

    /// <summary>
    /// ユーザー ID（実行者）
    /// </summary>
    [MaxLength(50)]
    public string? UserId { get; set; }

    /// <summary>
    /// 備考
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}

