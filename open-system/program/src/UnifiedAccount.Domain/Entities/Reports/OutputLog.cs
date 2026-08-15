using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 出力ログ (KOZXLG+CHOXLG) — ILogger置換過渡期用
/// </summary>
[Table("TD_OutputLogs")]
[Index(nameof(LogDate))]
public class OutputLog : BaseEntity
{
    public DateTime LogDate { get; set; } = LocalDateTimeProvider.Now;

    [Required, MaxLength(20)]
    public string Source { get; set; } = string.Empty;

    [Required, MaxLength(8)]
    public string ProgramName { get; set; } = string.Empty;

    [MaxLength(4)]
    /// <summary>
    /// メッセージコードを取得または設定する。
    /// </summary>
    public string? MessageCode { get; set; }

    [MaxLength(200)]
    /// <summary>
    /// メッセージテキストを取得または設定する。
    /// </summary>
    public string? MessageText { get; set; }

    /// <summary>
    /// レコード件数を取得または設定する。
    /// </summary>
    public int? RecordCount { get; set; }
}

