using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 契約者マスター (KOZXKM+KOZXKV+KOZXKK+KOZXUK)
/// </summary>
[Table("TM_Contracts")]
[Index(nameof(CompanyCode), nameof(PersonalCode), nameof(CheckDigit))]
[Index(nameof(CompanyCode), nameof(PersonalCode), nameof(ContractSeq))]
[Index(nameof(BankCode), nameof(BranchCode), nameof(AccountType), nameof(AccountNo))]
[Index(nameof(DepositorNameKana))]
[Index(nameof(PersonalCode))]
public class Contract : BaseEntity
{
    /// <summary>
    /// 会社IDを取得または設定する。
    /// </summary>
    public long CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    /// <summary>
    /// 銀行支店IDを取得または設定する。
    /// </summary>
    public long? BankBranchId { get; set; }
    /// <summary>
    /// 銀行支店を取得または設定する。
    /// </summary>
    public BankBranch? BankBranch { get; set; }

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PersonalCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string CheckDigit { get; set; } = string.Empty;

    /// <summary>
    /// 契約連番を取得または設定する。
    /// </summary>
    /// <remarks>
    /// 移行元は KOZXKV.KV-G-SEQ 9(4)。
    /// 同一 会社コード + 個人コード 内を CheckDigit, Id の昇順で 1 起点に採番する。
    /// 確定処理の直後、INSERT が発生したグループのみ再採番する。
    /// </remarks>
    public short ContractSeq { get; set; }

    [Required, MaxLength(1)]
    public string CodeSave { get; set; } = " ";

    /// <summary>
    /// 引落日を取得または設定する。
    /// </summary>
    public short WithdrawalDay { get; set; }

    [MaxLength(6)]
    /// <summary>
    /// 開始年月を取得または設定する。
    /// </summary>
    public string? StartYearMonth { get; set; }

    [Required, MaxLength(1)]
    public string SuspendFlag { get; set; } = "0";

    [Required, MaxLength(1)]
    public string NewFlag { get; set; } = "0";

    [Required, MaxLength(1)]
    public string ZenginFlag { get; set; } = "0";

    [Required, MaxLength(1)]
    public string NotifiedFlag { get; set; } = "0";

    [Required, MaxLength(1)]
    public string ResultFlag { get; set; } = "0";

    [Required, MaxLength(1)]
    public string ProcessType { get; set; } = "0";

    [Required, MaxLength(1)]
    public string CreditCompleteFlag { get; set; } = "0";

    [Required, MaxLength(1)]
    public string TransferMethod { get; set; } = "0";

    [Required, MaxLength(1)]
    public string AutoDeleteFlag { get; set; } = "0";

    /// <summary>
    /// 失敗件数を取得または設定する。
    /// </summary>
    public short FailureCount { get; set; }

    [Required, MaxLength(32)]
    public string DepositorNameKana { get; set; } = string.Empty;

    [MaxLength(32)]
    /// <summary>
    /// 契約者名称カナを取得または設定する。
    /// </summary>
    public string? ContractorNameKana { get; set; }

    [MaxLength(20)]
    /// <summary>
    /// 預金者名称漢字を取得または設定する。
    /// </summary>
    public string? DepositorNameKanji { get; set; }

    [MaxLength(20)]
    /// <summary>
    /// 契約者名称漢字を取得または設定する。
    /// </summary>
    public string? ContractorNameKanji { get; set; }

    [MaxLength(7)]
    /// <summary>
    /// 郵便コードを取得または設定する。
    /// </summary>
    public string? PostalCode { get; set; }

    [MaxLength(9)]
    /// <summary>
    /// 都道府県を取得または設定する。
    /// </summary>
    public string? Prefecture { get; set; }

    [MaxLength(20)]
    /// <summary>
    /// 市区町村を取得または設定する。
    /// </summary>
    public string? City { get; set; }

    [MaxLength(24)]
    /// <summary>
    /// 町域1を取得または設定する。
    /// </summary>
    public string? Town1 { get; set; }

    [MaxLength(24)]
    /// <summary>
    /// 町域2を取得または設定する。
    /// </summary>
    public string? Town2 { get; set; }

    [MaxLength(7)]
    /// <summary>
    /// 電話番号を取得または設定する。
    /// </summary>
    public string? PhoneNumber { get; set; }

    [Required, MaxLength(4)]
    public string BankCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string BranchCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string AccountType { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string AccountNo { get; set; } = string.Empty;

    [MaxLength(10)]
    /// <summary>
    /// 入金商品名称を取得または設定する。
    /// </summary>
    public string? CreditProductName { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 入金合計金額を取得または設定する。
    /// </summary>
    public decimal? CreditTotalAmount { get; set; }

    /// <summary>
    /// 入金合計件数を取得または設定する。
    /// </summary>
    public short? CreditTotalCount { get; set; }
    /// <summary>
    /// 入金完了件数を取得または設定する。
    /// </summary>
    public short? CreditCompletedCount { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 入金入金1を取得または設定する。
    /// </summary>
    public decimal? CreditPayment1 { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 入金入金2を取得または設定する。
    /// </summary>
    public decimal? CreditPayment2 { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 入金特別加算を取得または設定する。
    /// </summary>
    public decimal? CreditSpecialAddition { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 入金請求金額を取得または設定する。
    /// </summary>
    public decimal? CreditBillingAmount { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 前回残高を取得または設定する。
    /// </summary>
    public decimal PreviousBalance { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 当回請求金額を取得または設定する。
    /// </summary>
    public decimal CurrentBillingAmount { get; set; }

    /// <summary>
    /// 引落日付を取得または設定する。
    /// </summary>
    public DateOnly? WithdrawalDate { get; set; }
    /// <summary>
    /// 銀行処理日付を取得または設定する。
    /// </summary>
    public DateOnly? BankProcessDate { get; set; }
    /// <summary>
    /// 免除日付を取得または設定する。
    /// </summary>
    public DateOnly? ExemptionDate { get; set; }

    [MaxLength(1)]
    /// <summary>
    /// 免除種別を取得または設定する。
    /// </summary>
    public string? ExemptionType { get; set; }

    /// <summary>
    /// 変更日付を取得または設定する。
    /// </summary>
    public DateOnly? ChangeDate { get; set; }

    [MaxLength(1)]
    /// <summary>
    /// 異動区分を取得または設定する。'1'=削除、'2'=新規、'3'=修正。
    /// </summary>
    public string? ChangeAction { get; set; }

    [Precision(5, 2)]
    /// <summary>
    /// 請求書税税率1を取得または設定する。
    /// </summary>
    public decimal? InvoiceTaxRate1 { get; set; }

    [Precision(5, 2)]
    /// <summary>
    /// 請求書税税率2を取得または設定する。
    /// </summary>
    public decimal? InvoiceTaxRate2 { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 請求書基準単価を取得または設定する。
    /// </summary>
    public decimal? InvoiceBasePrice { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 請求書消費税税を取得または設定する。
    /// </summary>
    public decimal? InvoiceConsumptionTax { get; set; }

    // Navigation
    public ICollection<ContractType> Types { get; set; } = [];
    public ICollection<ContractBillingAmount> BillingAmounts { get; set; } = [];
    public ICollection<TransferFailure> TransferFailures { get; set; } = [];
}

