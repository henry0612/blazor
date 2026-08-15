using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Changes;

/// <summary>
/// 銀行異動リクエスト (伝区01)
/// </summary>
[Table("TD_BankChangeRequests")]
[Index(nameof(RequestType))]
[Index(nameof(BankCode), nameof(BranchCode))]
[Index(nameof(JobExecutionId), nameof(BatchStatus))]
public class BankChangeRequest : BaseEntity
{
    [Required, MaxLength(2)]
    public string RequestType { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string ChangeAction { get; set; } = string.Empty;

    [MaxLength(4)] public string? BankCode { get; set; }
    [MaxLength(3)] public string? BranchCode { get; set; }
    [MaxLength(20)] public string? BankName { get; set; }
    [MaxLength(20)] public string? BranchName { get; set; }

    [Required, MaxLength(15)]
    public string ErrorFlags { get; set; } = "000000000000000";

    [Required, MaxLength(50)]
    public string JobExecutionId { get; set; } = string.Empty;

    [Required, MaxLength(16)]
    public string BatchStatus { get; set; } = "SUCCESS";

    [MaxLength(20)]
    /// <summary>
    /// 失敗実行者プログラムを取得または設定する。
    /// </summary>
    public string? FailedByProgram { get; set; }

    [MaxLength(4)]
    /// <summary>
    /// 失敗理由コードを取得または設定する。
    /// </summary>
    public string? FailedReasonCode { get; set; }

    /// <summary>
    /// 失敗日時を取得または設定する。
    /// </summary>
    public DateTime? FailedAt { get; set; }

    public int Version { get; set; } = 1;
}


