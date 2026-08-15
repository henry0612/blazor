namespace UnifiedAccount.Domain.Interfaces;

/// <summary>
/// 営業日計算サービス (CALCNV相当)
/// </summary>
public interface ICalendarService
{
    Task<DateOnly> GetNextBusinessDayAsync(DateOnly date, CancellationToken ct = default);
    Task<DateOnly> GetPreviousBusinessDayAsync(DateOnly date, CancellationToken ct = default);
    Task<bool> IsBusinessDayAsync(DateOnly date, CancellationToken ct = default);
    Task<bool> IsHolidayAsync(DateOnly date, CancellationToken ct = default);
    Task<DateOnly> AdjustToBusinessDayAsync(DateOnly date, bool forward = true, CancellationToken ct = default);

    /// <summary>
    /// 指定日から businessDays 営業日分だけ進めた（または戻した）日付を返す。
    /// businessDays が正の場合は加算、負の場合は減算。0 の場合は date をそのまま返す。
    /// 非営業日（土日・祝祭日・12/31・1/2・1/3）をスキップしてカウントする。
    /// COBOL: ZGNS03 (ZGN-DATE + ZGN-UPDAY → ZGN-DATE) 相当。
    /// </summary>
    Task<DateOnly> AddBusinessDaysAsync(DateOnly date, int businessDays, CancellationToken ct = default);
}
