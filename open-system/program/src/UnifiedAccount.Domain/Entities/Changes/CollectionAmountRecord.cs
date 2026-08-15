using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Changes;

/// <summary>
/// 収納金額レコード (DENK=23 生成物) — KOZ012 AutoAmountSettingJob の出力エンティティ
/// </summary>
[Table("TD_CollectionAmountRecords")]
public class CollectionAmountRecord : BaseEntity
{
    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PersonalCode { get; set; } = string.Empty;

    /// <summary>
    /// 異動種別 (DENK=23 固定)
    /// </summary>
    [MaxLength(2)]
    public string ChangeType { get; set; } = "23";

    /// <summary>
    /// 異動区分 (IDOK=2 固定)
    /// </summary>
    [MaxLength(1)]
    public string? ChangeAction { get; set; } = "2";

    /// <summary>
    /// 種別番号
    /// </summary>
    public short TypeNo { get; set; }

    /// <summary>
    /// 種別コード
    /// </summary>
    [Required, MaxLength(1)]
    public string TypeCode { get; set; } = string.Empty;

    /// <summary>
    /// 金額
    /// </summary>
    [Precision(10, 0)]
    public decimal Amount { get; set; }
}

