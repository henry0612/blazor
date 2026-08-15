using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Zengin;

/// <summary>
/// 全銀照合データ (KOZXZS)
/// </summary>
[Table("TD_ZenginVerifications", Schema = "zengin")]
[Index(nameof(TransferDate))]
public class ZenginVerification : BaseEntity
{
    /// <summary>
    /// 振替日付を取得または設定する。
    /// </summary>
    public DateOnly TransferDate { get; set; }

    [Required, MaxLength(2)]
    public string CycleCode { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    public string VerificationCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string CancelFlag { get; set; } = "0";

    [MaxLength(10)]
    /// <summary>
    /// 依頼者コードを取得または設定する。
    /// </summary>
    public string? RequestorCode { get; set; }

    /// <summary>
    /// 合計件数を取得または設定する。
    /// </summary>
    public int TotalCount { get; set; }

    [Precision(12, 0)]
    /// <summary>
    /// 合計金額を取得または設定する。
    /// </summary>
    public decimal TotalAmount { get; set; }
}


