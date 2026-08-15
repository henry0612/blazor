using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 処理カレンダー (KOZXCL)
/// </summary>
[Table("TM_ProcessingCalendars")]
[Index(nameof(ProcessingDate), IsUnique = true)]
public class ProcessingCalendar : BaseEntity
{
    /// <summary>
    /// 処理日付を取得または設定する。
    /// </summary>
    public DateOnly ProcessingDate { get; set; }

    // Navigation
    public ICollection<ProcessingSlot> Slots { get; set; } = [];
}


