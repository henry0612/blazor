using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 振替トランザクション (KOZXHD)
/// </summary>
[Table("TD_TransferTransactions")]
[Index(nameof(WithdrawalDate))]
[Index(nameof(CompanyId), nameof(WithdrawalDate))]
[Index(nameof(CompanyCode), nameof(PersonalCode))]
public class TransferTransaction : BaseEntity
{
    /// <summary>
    /// 会社IDを取得または設定する。
    /// </summary>
    public long? CompanyId { get; set; }
    /// <summary>
    /// 会社を取得または設定する。
    /// </summary>
    public Company? Company { get; set; }

    [Required, MaxLength(10)]
    public string ConsignorCode { get; set; } = string.Empty;

    [MaxLength(40)]
    /// <summary>
    /// 委託者名称を取得または設定する。
    /// </summary>
    public string? ConsignorName { get; set; }

    /// <summary>
    /// 引落日付を取得または設定する。
    /// </summary>
    public DateOnly WithdrawalDate { get; set; }

    [Required, MaxLength(4)]
    public string BankCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string BranchCode { get; set; } = string.Empty;

    [MaxLength(15)]
    /// <summary>
    /// 銀行名称を取得または設定する。
    /// </summary>
    public string? BankName { get; set; }

    [MaxLength(15)]
    /// <summary>
    /// 支店名称を取得または設定する。
    /// </summary>
    public string? BranchName { get; set; }

    [Required, MaxLength(1)]
    public string AccountType { get; set; } = string.Empty;

    [Required, MaxLength(10)]
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

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PersonalCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string CheckDigit { get; set; } = string.Empty;

    [MaxLength(1)]
    /// <summary>
    /// 結果コードを取得または設定する。
    /// </summary>
    public string? ResultCode { get; set; }

    [MaxLength(8)]
    /// <summary>
    /// 通帳コメントを取得または設定する。
    /// </summary>
    public string? PassbookComment { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 種別1金額を取得または設定する。
    /// </summary>
    public decimal Type1Amount { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 種別2金額を取得または設定する。
    /// </summary>
    public decimal Type2Amount { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 種別3金額を取得または設定する。
    /// </summary>
    public decimal Type3Amount { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 種別4金額を取得または設定する。
    /// </summary>
    public decimal Type4Amount { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 入金金額を取得または設定する。
    /// </summary>
    public decimal CreditAmount { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 前回残高を取得または設定する。
    /// </summary>
    public decimal PreviousBalance { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 当回請求を取得または設定する。
    /// </summary>
    public decimal CurrentBilling { get; set; }

    [MaxLength(30)]
    /// <summary>
    /// 契約者名称を取得または設定する。
    /// </summary>
    public string? ContractorName { get; set; }

    [MaxLength(4)]
    /// <summary>
    /// 振替口座銀行コードを取得または設定する。
    /// </summary>
    public string? TransferAccountBankCode { get; set; }

    [MaxLength(3)]
    /// <summary>
    /// 振替口座支店コードを取得または設定する。
    /// </summary>
    public string? TransferAccountBranchCode { get; set; }

    [MaxLength(10)]
    /// <summary>
    /// 振替口座番号を取得または設定する。
    /// </summary>
    public string? TransferAccountNo { get; set; }

    /// <summary>
    /// 振替回 (JCL RUNJOB → KUBUN相当: 1=JOB1, 2=JOB2, 3=JOB3)
    /// 同一WithdrawalDateに対して複数回の振替試行が発生する場合に回次を区別する。
    /// </summary>
    public int TransferRound { get; set; } = 1;
}


