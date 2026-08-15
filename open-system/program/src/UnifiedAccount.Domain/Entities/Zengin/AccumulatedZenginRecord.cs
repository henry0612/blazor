using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Zengin;

/// <summary>
/// 全銀累積データ (KOZXZC)
/// </summary>
[Table("TD_AccumulatedZenginRecords", Schema = "zengin")]
[Index(nameof(BankCode), nameof(BranchCode), nameof(AccountNo))]
public class AccumulatedZenginRecord : BaseEntity
{
    [Required, MaxLength(4)]
    public string BankCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string BranchCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string AccountType { get; set; } = string.Empty;

    [Required, MaxLength(7)]
    public string AccountNo { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string DepositorName { get; set; } = string.Empty;

    [Precision(10, 0)]
    /// <summary>
    /// 金額を取得または設定する。
    /// </summary>
    public decimal Amount { get; set; }

    [MaxLength(1)] public string? ResultCode { get; set; }
    [MaxLength(10)] public string? ConsignorCode { get; set; }
    /// <summary>
    /// 引落日付を取得または設定する。
    /// </summary>
    public DateOnly? WithdrawalDate { get; set; }
    [MaxLength(15)] public string? BankName { get; set; }
    [MaxLength(15)] public string? BranchName { get; set; }
    [MaxLength(8)] public string? PassbookComment { get; set; }
}


