using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 和暦元号マスター — 元号の名称・期間・コードをDB管理する。
/// 新元号への対応はこのテーブルにレコードを追加するだけで完了する。
/// </summary>
[Table("TM_JapaneseEras")]
[Index(nameof(Code), IsUnique = true)]
[Index(nameof(StartDate))]
public class JapaneseEra : BaseEntity
{
    /// <summary>
    /// 元号コード (1=明治, 2=大正, 3=昭和, 4=平成, 5=令和)
    /// </summary>
    [Required]
    public int Code { get; set; }

    /// <summary>
    /// 元号名称 (漢字: 令和, 平成, ...)
    /// </summary>
    [Required, MaxLength(4)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 元号名称略称 (アルファベット: R, H, S, T, M)
    /// </summary>
    [Required, MaxLength(1)]
    public string Abbreviation { get; set; } = string.Empty;

    /// <summary>
    /// 開始日 (元号初日)
    /// </summary>
    [Required]
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// 終了日 (元号最終日, 現在の元号はnull)
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// 基準年 (西暦 - 元号年 = 基準年。例: 令和=2018, 平成=1988)
    /// </summary>
    [Required]
    public int BaseYear { get; set; }
}

