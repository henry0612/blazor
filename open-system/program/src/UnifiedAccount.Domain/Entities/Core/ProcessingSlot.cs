using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Core;

/// <summary>
/// 処理スロット (CL-SYORI OCCURS 3)
/// </summary>
[Table("TM_ProcessingSlots")]
public class ProcessingSlot : BaseEntity
{
    /// <summary>
    /// 処理カレンダーIDを取得または設定する。
    /// </summary>
    public long ProcessingCalendarId { get; set; }
    public ProcessingCalendar ProcessingCalendar { get; set; } = null!;

    /// <summary>
    /// 枠番号を取得または設定する。
    /// </summary>
    public short SlotNo { get; set; }

    [Required, MaxLength(1)]
    public string ProcessingType { get; set; } = "0";

    /// <summary>
    /// 引落日付1を取得または設定する。
    /// </summary>
    public DateOnly? WithdrawalDate1 { get; set; }
    /// <summary>
    /// 引落日付2を取得または設定する。
    /// </summary>
    public DateOnly? WithdrawalDate2 { get; set; }
    /// <summary>
    /// 引落日付3を取得または設定する。
    /// </summary>
    public DateOnly? WithdrawalDate3 { get; set; }

    /// <summary>
    /// 完了フラグを取得または設定する。
    /// </summary>
    public bool CompletedFlag { get; set; }
}


