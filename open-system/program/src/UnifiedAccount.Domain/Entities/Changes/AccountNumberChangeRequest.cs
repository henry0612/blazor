using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Changes;

/// <summary>
/// 口座番号変更異動リクエスト (伝区20)
/// </summary>
[Table("TD_AccountNumberChangeRequests")]
[Index(nameof(RequestType))]
[Index(nameof(JobExecutionId), nameof(BatchStatus))]
public class AccountNumberChangeRequest : BaseEntity
{
    [Required, MaxLength(2)]
    public string RequestType { get; set; } = "20";

    [Required, MaxLength(1)]
    public string ChangeAction { get; set; } = string.Empty;

    [MaxLength(4)] public string? OldBankCode { get; set; }
    [MaxLength(3)] public string? OldBranchCode { get; set; }
    [MaxLength(1)] public string? OldAccountType { get; set; }
    [MaxLength(10)] public string? OldAccountNumber { get; set; }
    [MaxLength(3)] public string? NewBranchCode { get; set; }
    [MaxLength(1)] public string? NewAccountType { get; set; }
    [MaxLength(10)] public string? NewAccountNumber { get; set; }

    [Required, MaxLength(15)]
    public string ErrorFlags { get; set; } = "000000000000000";

    [Required, MaxLength(50)]
    public string JobExecutionId { get; set; } = string.Empty;

    [Required, MaxLength(16)]
    public string BatchStatus { get; set; } = "SUCCESS";

    [MaxLength(20)] public string? FailedByProgram { get; set; }
    [MaxLength(4)] public string? FailedReasonCode { get; set; }
    public DateTime? FailedAt { get; set; }
    public int Version { get; set; } = 1;
}
