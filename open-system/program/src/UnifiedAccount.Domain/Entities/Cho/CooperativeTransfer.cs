using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Cho;

/// <summary>
/// 共済振替データ (CHOXZK)
/// </summary>
[Table("TD_CooperativeTransfers", Schema = "cho")]
[Index(nameof(MemberCode))]
[Index(nameof(RecordType))]
[Index(nameof(KozConsignorCode), nameof(MatchYearMonth), nameof(MatchSeqNo))]
public class CooperativeTransfer : BaseEntity
{
    [Required, MaxLength(1)]
    public string RecordType { get; set; } = string.Empty;

    [MaxLength(10)] public string? ConsignorCode { get; set; }
    [MaxLength(40)] public string? ConsignorNameKana { get; set; }
    [MaxLength(2)] public string? WithdrawalMonth { get; set; }
    [MaxLength(2)] public string? WithdrawalDay { get; set; }
    [MaxLength(4)] public string? BankCode { get; set; }
    [MaxLength(3)] public string? BranchCode { get; set; }
    [MaxLength(4)] public string? ContractNo { get; set; }
    [MaxLength(6)] public string? MemberCode { get; set; }
    [MaxLength(4)] public string? PlanCode { get; set; }
    [MaxLength(3)] public string? CooperativeNo { get; set; }
    [MaxLength(3)] public string? BranchOfficeNo { get; set; }
    [MaxLength(2)] public string? InsuranceType { get; set; }
    [MaxLength(1)] public string? PaymentMethod { get; set; }
    /// <summary>
    /// 契約日付を取得または設定する。
    /// </summary>
    public DateOnly? ContractDate { get; set; }
    [MaxLength(1)] public string? AnnualMonthlyType { get; set; }
    [MaxLength(1)] public string? AccountType { get; set; }
    [MaxLength(7)] public string? AccountNo { get; set; }
    [MaxLength(30)] public string? DepositorName { get; set; }
    [Precision(10, 0)] public decimal? Amount { get; set; }
    [MaxLength(1)] public string? NewCode { get; set; }
    [MaxLength(14)] public string? InvariantNo { get; set; }
    [MaxLength(6)] public string? PremiumYearMonth { get; set; }
    [MaxLength(1)] public string? ResultCode { get; set; }
    [MaxLength(4)] public string? PostalBankCode { get; set; }
    [MaxLength(3)] public string? PostalBranchCode { get; set; }
    [MaxLength(1)] public string? PostalAccountType { get; set; }
    [MaxLength(10)] public string? PostalAccountNo { get; set; }
    [MaxLength(6)] public string? KozConsignorCode { get; set; }
    [MaxLength(6)] public string? MatchYearMonth { get; set; }
    /// <summary>
    /// 突合順序番号を取得または設定する。
    /// </summary>
    public int? MatchSeqNo { get; set; }
    /// <summary>
    /// 合計件数を取得または設定する。
    /// </summary>
    public int? TotalCount { get; set; }
    [Precision(12, 0)] public decimal? TotalAmount { get; set; }
    /// <summary>
    /// 決済済件数を取得または設定する。
    /// </summary>
    public int? SettledCount { get; set; }
    [Precision(12, 0)] public decimal? SettledAmount { get; set; }
    /// <summary>
    /// 失敗件数を取得または設定する。
    /// </summary>
    public int? FailedCount { get; set; }
    [Precision(12, 0)] public decimal? FailedAmount { get; set; }
}


