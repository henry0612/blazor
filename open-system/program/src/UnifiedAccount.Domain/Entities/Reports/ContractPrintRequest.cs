using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 帳票実行管理 (TR_ContractPrintRequests)
/// </summary>
[Table("TR_ContractPrintRequests")]
[Index(nameof(JobExecutionId), nameof(RequestOrder), IsUnique = true)]
[Index(nameof(JobExecutionId))]
[Index(nameof(Status))]
public class ContractPrintRequest : BaseEntity
{
    [Required, MaxLength(50)]
    public string JobExecutionId { get; set; } = string.Empty;

    [Required]
    /// <summary>
    /// リクエスト順序を取得または設定する。
    /// </summary>
    public short RequestOrder { get; set; }

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PersonCodeFrom { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PersonCodeTo { get; set; } = string.Empty;

    [Required]
    /// <summary>
    /// 全体会社フラグを取得または設定する。
    /// </summary>
    public bool AllCompanyFlag { get; set; }

    [Required, MaxLength(1)]
    public string Status { get; set; } = "0";

    [Required]
    /// <summary>
    /// 印刷済件数を取得または設定する。
    /// </summary>
    public int PrintedCount { get; set; }

    [Required]
    /// <summary>
    /// 監査件数を取得または設定する。
    /// </summary>
    public int AuditCount { get; set; }
}

