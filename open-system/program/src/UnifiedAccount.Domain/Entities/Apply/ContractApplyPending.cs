using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Apply;

/// <summary>
/// 契約者マスタ確定前差分 (新規)
/// </summary>
/// <remarks>
/// 現行 KMNEW に相当する。適用フェーズが変更行のみを生成し、確定フェーズが
/// TM_Contracts へ MERGE する。試行ごとに作り直されるため更新されない。
/// Contract.ContractSeq は確定処理直後に会社＋個人グループ単位で
/// 再採番される値であり、確定前の本エンティティでは値が定まらないため保持しない。
/// </remarks>
[Table("TD_ContractApplyPending")]
[Index(nameof(ApplyRunId), nameof(CompanyCode), nameof(PersonalCode), nameof(CheckDigit))]
public class ContractApplyPending : BaseEntity
{
    /// <summary>
    /// 適用試行IDを取得または設定する。
    /// </summary>
    public long ApplyRunId { get; set; }

    /// <summary>
    /// 適用試行を取得または設定する。
    /// </summary>
    public ApplyRun? ApplyRun { get; set; }

    /// <summary>
    /// 反映元を取得または設定する。CHANGE_REQUEST / ZENGIN_IMPORT のいずれか。
    /// </summary>
    [Required, MaxLength(16)]
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// 操作を取得または設定する。INSERT / UPDATE のいずれか。
    /// 契約者は論理削除のため DELETE を持たない。
    /// </summary>
    [Required, MaxLength(8)]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// 契約IDを取得または設定する。UPDATE 時に設定し、INSERT では null とする。
    /// </summary>
    public long? ContractId { get; set; }

    /// <summary>
    /// 異動データIDを取得または設定する。Source が CHANGE_REQUEST のとき設定する。
    /// </summary>
    public long? SourceRequestId { get; set; }

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PersonalCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string CheckDigit { get; set; } = string.Empty;

    /// <summary>
    /// 検証結果フラグを取得または設定する。この試行での検証結果を保持する。
    /// </summary>
    [Required, MaxLength(15)]
    public string ValidationFlags { get; set; } = "000000000000000";

    /// <summary>
    /// 適用可否を取得または設定する。false の行は確定対象から外す。
    /// </summary>
    public bool IsApplicable { get; set; } = true;

    // ---- 以下、TM_Contracts の業務列 ----

    /// <summary>会社IDを取得または設定する。</summary>
    public long CompanyId { get; set; }

    /// <summary>銀行支店IDを取得または設定する。</summary>
    public long? BankBranchId { get; set; }

    [Required, MaxLength(1)]
    public string CodeSave { get; set; } = " ";

    /// <summary>引落日を取得または設定する。</summary>
    public short WithdrawalDay { get; set; }

    [MaxLength(6)]
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

    /// <summary>不能回数を取得または設定する。</summary>
    public short FailureCount { get; set; }

    [Required, MaxLength(32)]
    public string DepositorNameKana { get; set; } = string.Empty;

    // Contract.ContractorNameKana (MaxLength 32) に合わせて列名を揃えている。
    // Contract 側に ContractorName という同名列はなく、ContractorNameKana が対応する業務列である。
    [MaxLength(32)]
    public string? ContractorNameKana { get; set; }

    [MaxLength(20)]
    public string? DepositorNameKanji { get; set; }

    [MaxLength(20)]
    public string? ContractorNameKanji { get; set; }

    [Required, MaxLength(4)]
    public string BankCode { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string BranchCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string AccountType { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string AccountNo { get; set; } = string.Empty;

    [MaxLength(7)]
    public string? PostalCode { get; set; }

    [MaxLength(9)]
    public string? Prefecture { get; set; }

    [MaxLength(20)]
    public string? City { get; set; }

    [MaxLength(24)]
    public string? Town1 { get; set; }

    [MaxLength(24)]
    public string? Town2 { get; set; }

    [MaxLength(7)]
    public string? PhoneNumber { get; set; }

    // ---- 種別（TM_ContractTypes の OCCURS 4 を横展開）----

    [MaxLength(6)] public string? Type1StartYearMonth { get; set; }
    [MaxLength(2)] public string? Type1Cycle { get; set; }
    [Precision(10, 0)] public decimal Type1Amount { get; set; }

    [MaxLength(6)] public string? Type2StartYearMonth { get; set; }
    [MaxLength(2)] public string? Type2Cycle { get; set; }
    [Precision(10, 0)] public decimal Type2Amount { get; set; }

    [MaxLength(6)] public string? Type3StartYearMonth { get; set; }
    [MaxLength(2)] public string? Type3Cycle { get; set; }
    [Precision(10, 0)] public decimal Type3Amount { get; set; }

    [MaxLength(6)] public string? Type4StartYearMonth { get; set; }
    [MaxLength(2)] public string? Type4Cycle { get; set; }
    [Precision(10, 0)] public decimal Type4Amount { get; set; }

    // ---- 請求額（TM_ContractBillingAmounts の OCCURS 4 を横展開）----

    [Precision(10, 0)] public decimal Billing1Amount { get; set; }
    [Precision(10, 0)] public decimal Billing2Amount { get; set; }
    [Precision(10, 0)] public decimal Billing3Amount { get; set; }
    [Precision(10, 0)] public decimal Billing4Amount { get; set; }

    // ---- クレジット ----

    [MaxLength(10)] public string? CreditProductName { get; set; }
    [Precision(10, 0)] public decimal? CreditTotalAmount { get; set; }
    public short? CreditTotalCount { get; set; }
    public short? CreditCompletedCount { get; set; }
    [Precision(10, 0)] public decimal? CreditPayment1 { get; set; }
    [Precision(10, 0)] public decimal? CreditPayment2 { get; set; }
    [Precision(10, 0)] public decimal? CreditSpecialAddition { get; set; }
    [Precision(10, 0)] public decimal? CreditBillingAmount { get; set; }

    // ---- 残高・請求（TM_Contracts の状態列）----

    [Precision(10, 0)] public decimal PreviousBalance { get; set; }
    [Precision(10, 0)] public decimal CurrentBillingAmount { get; set; }

    // ---- 状態・異動（TM_Contracts の状態列）----

    public DateOnly? WithdrawalDate { get; set; }
    public DateOnly? BankProcessDate { get; set; }
    public DateOnly? ExemptionDate { get; set; }
    [MaxLength(1)] public string? ExemptionType { get; set; }
    public DateOnly? ChangeDate { get; set; }
    [MaxLength(1)] public string? ChangeAction { get; set; }

    // ---- 請求書（TM_Contracts の請求書関連列）----

    [Precision(5, 2)] public decimal? InvoiceTaxRate1 { get; set; }
    [Precision(5, 2)] public decimal? InvoiceTaxRate2 { get; set; }
    [Precision(10, 0)] public decimal? InvoiceBasePrice { get; set; }
    [Precision(10, 0)] public decimal? InvoiceConsumptionTax { get; set; }
}
