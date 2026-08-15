namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// カレンダーDTO
/// </summary>
public record CalendarDto
{
    /// <summary>
    /// IDを取得または設定する。
    /// </summary>
    public long Id { get; init; }
    /// <summary>
    /// 処理日付を取得または設定する。
    /// </summary>
    public DateOnly ProcessingDate { get; init; }
    /// <summary>
    /// 業務日を取得または設定する。
    /// </summary>
    public bool IsBusinessDay { get; init; }
    public IReadOnlyList<CalendarSlotDto> Slots { get; init; } = [];
}

/// <summary>
/// CalendarSlotDto を表すレコード。
/// </summary>
public record CalendarSlotDto
{
    /// <summary>
    /// 枠番号を取得または設定する。
    /// </summary>
    public short SlotNo { get; init; }
    public string ProcessingType { get; init; } = string.Empty;
    /// <summary>
    /// 引落日付1を取得または設定する。
    /// </summary>
    public DateOnly? WithdrawalDate1 { get; init; }
    /// <summary>
    /// 完了フラグを取得または設定する。
    /// </summary>
    public bool CompletedFlag { get; init; }
}



