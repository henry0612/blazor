using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 振替不能データ (KOZXFD+KOZXFV)
/// </summary>
[Table("TD_TransferFailures")]
[Index(nameof(WithdrawalDay), nameof(TransferType), nameof(CompanyCode), nameof(PersonalCode), nameof(CheckDigit), IsUnique = true)]
public class TransferFailure : BaseEntity
{
    /// <summary>
    /// 契約IDを取得または設定する。
    /// </summary>
    public long? ContractId { get; set; }
    /// <summary>
    /// 契約を取得または設定する。
    /// </summary>
    public Contract? Contract { get; set; }

    [Required, MaxLength(2)]
    public string WithdrawalDay { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    public string TransferType { get; set; } = string.Empty;

    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PersonalCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string CheckDigit { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string ResultCode { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string TapeType { get; set; } = "0";

    [Required, MaxLength(4)]
    public string BankCode { get; set; } = string.Empty;

    /// <summary>
    /// 振替回 (1=JOB1, 2=JOB2, 3=JOB3)
    /// 再振替対象の選定時 (KOZ045) に使用。JOB2はRound1不能分を、JOB3はRound2不能分を対象とする。
    /// </summary>
    public int TransferRound { get; set; } = 1;
}


