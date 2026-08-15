using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Cho;

/// <summary>
/// 処理指示 (CHOXIS)
/// </summary>
[Table("TD_ProcessingInstructions", Schema = "cho")]
[Index(nameof(SettlementType), nameof(TargetYearMonth), IsUnique = true)]
public class ProcessingInstruction : BaseEntity
{
    [Required, MaxLength(2)]
    public string SettlementType { get; set; } = string.Empty;

    /// <summary>
    /// 対象年月を取得または設定する。
    /// </summary>
    public DateOnly TargetYearMonth { get; set; }

    [MaxLength(1)]
    /// <summary>
    /// 経理結果種別を取得または設定する。
    /// </summary>
    public string? AccountingResultType { get; set; }

    /// <summary>
    /// 業務更新日付を取得または設定する。
    /// </summary>
    public DateOnly? BusinessUpdateDate { get; set; }
    /// <summary>
    /// 経理更新日付を取得または設定する。
    /// </summary>
    public DateOnly? AccountingUpdateDate { get; set; }
}


