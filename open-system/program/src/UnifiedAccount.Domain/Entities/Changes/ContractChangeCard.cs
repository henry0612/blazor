using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Changes;

/// <summary>
/// 契約者カード入力 (KOZXCD)
/// </summary>
[Table("TD_ContractChangeCards")]
[Index(nameof(ContractCode))]
public class ContractChangeCard : BaseEntity
{
    [Required, MaxLength(2)]
    public string CardType { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string CardSubType { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string ChangeAction { get; set; } = string.Empty;

    [Required, MaxLength(18)]
    public string ContractCode { get; set; } = string.Empty;

    [MaxLength(32)] public string? DepositorName { get; set; }
    [MaxLength(18)] public string? AccountInfo { get; set; }
    [MaxLength(2)] public string? WithdrawalDay { get; set; }
    [MaxLength(4)] public string? StartYearMonth { get; set; }
    [MaxLength(1)] public string? SuspendFlag { get; set; }
    [MaxLength(1)] public string? TypeNo { get; set; }
    [MaxLength(9)] public string? AmountText { get; set; }
    [MaxLength(4)] public string? NextYearMonth { get; set; }
    [MaxLength(10)] public string? CreditProductName { get; set; }
    [MaxLength(9)] public string? CreditTotalAmountText { get; set; }
    [MaxLength(2)] public string? CreditTotalCountText { get; set; }
}

