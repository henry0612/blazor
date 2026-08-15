using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// バッチ/帳票ジョブの実行履歴。
/// </summary>
[Table("TD_BatchExecutionHistories")]
[Index(nameof(JobExecutionId), IsUnique = true, Name = "UQ_TD_BatchExecutionHistories_JobExecutionId")]
[Index(nameof(Status), nameof(CompletedAt), nameof(FunctionId), Name = "IX_TD_BatchExecutionHistories_Status")]
public class BatchExecutionHistory : BaseEntity
{
    [Required, MaxLength(50)]
    public string JobExecutionId { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string FunctionId { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string JobId { get; set; } = string.Empty;

    public DateOnly ProcessDate { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [Required, MaxLength(16)]
    public string Status { get; set; } = string.Empty;

    public int ReadCount { get; set; }

    public int WriteCount { get; set; }

    public int ErrorCount { get; set; }

    [MaxLength(4000)]
    public string? Message { get; set; }
}
