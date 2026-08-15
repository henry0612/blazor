using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

// TODO: MF APA-FILE のデータが CompanyTypes と同一であることが確認できた場合、
// このテーブルを廃止し CompanyTypes を直接参照する統合を検討する。
// 現時点では MF の分離設計を尊重して独立テーブルとする。
/// <summary>
/// 金額自動セット設定 (MF APA-FILE 相当) — KOZ012 対象会社の種別×金額パラメータ
/// </summary>
[Table("TM_AmountSettings")]
[Index(nameof(CompanyCode), IsUnique = false)]
public class AmountSetting : BaseEntity
{
    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>
    /// 種別番号 (1-4, MF OCCURS 4)
    /// </summary>
    public short TypeNo { get; set; }

    /// <summary>
    /// 種別コード (APA-SHU)
    /// </summary>
    [Required, MaxLength(1)]
    public string TypeCode { get; set; } = string.Empty;

    /// <summary>
    /// 金額 (APA-KINGAKU)
    /// </summary>
    [Precision(10, 0)]
    public decimal Amount { get; set; }
}

