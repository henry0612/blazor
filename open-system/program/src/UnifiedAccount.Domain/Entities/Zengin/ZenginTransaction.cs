using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Zengin;

/// <summary>
/// 全銀データ明細 (KOZXZD DATA)
/// </summary>
[Table("TD_ZenginTransactions", Schema = "zengin")]
[Index(nameof(BankCode), nameof(BranchCode), nameof(AccountNo))]
[Index(nameof(ZenginBatchId))]
public class ZenginTransaction : BaseEntity
{
    /// <summary>
    /// 全銀バッチIDを取得または設定する。
    /// </summary>
    public long ZenginBatchId { get; set; }
    public ZenginBatch ZenginBatch { get; set; } = null!;

    [Required, MaxLength(4)]
    public string BankCode { get; set; } = string.Empty;

    [MaxLength(15)]
    /// <summary>
    /// 銀行名称を取得または設定する。
    /// </summary>
    public string? BankName { get; set; }

    [Required, MaxLength(3)]
    public string BranchCode { get; set; } = string.Empty;

    [MaxLength(15)]
    /// <summary>
    /// 支店名称を取得または設定する。
    /// </summary>
    public string? BranchName { get; set; }

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

    [Required, MaxLength(1)]
    public string NewCode { get; set; } = "0";

    [MaxLength(19)]
    /// <summary>
    /// 契約者コードを取得または設定する。
    /// </summary>
    public string? ContractorCode { get; set; }

    [MaxLength(1)]
    /// <summary>
    /// 結果コードを取得または設定する。
    /// </summary>
    public string? ResultCode { get; set; }
}


