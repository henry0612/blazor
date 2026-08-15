using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 契約種別 (KM-SHUBETSU OCCURS 4)
/// </summary>
[Table("TM_ContractTypes")]
public class ContractType : BaseEntity
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

    [MaxLength(6)]
    /// <summary>
    /// 開始年月を取得または設定する。
    /// </summary>
    public string? StartYearMonth { get; set; }

    [MaxLength(2)]
    /// <summary>
    /// 周期を取得または設定する。
    /// </summary>
    public string? Cycle { get; set; }

    [Precision(10, 0)]
    /// <summary>
    /// 金額を取得または設定する。
    /// </summary>
    public decimal Amount { get; set; }
}


