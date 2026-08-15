using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 契約請求額 (KM-SEIKYU OCCURS 4)
/// </summary>
[Table("TM_ContractBillingAmounts")]
public class ContractBillingAmount : BaseEntity
{
    /// <summary>
    /// 契約IDを取得または設定する。
    /// </summary>
    public long ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    /// <summary>
    /// 種別番号を取得または設定する。
    /// </summary>
    public short TypeNo { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 請求金額を取得または設定する。
    /// </summary>
    public decimal BillingAmount { get; set; }
}


