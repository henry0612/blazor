using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Interfaces;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Infrastructure.Services;

/// <summary>
/// 営業日計算サービス (CALCNV, KOZHLDYC 相当)
/// </summary>
public class CalendarService : ICalendarService
{
    /// <summary>
    /// コンテキストを保持する。
    /// </summary>
    private readonly AppDbContext _context;

    public CalendarService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DateOnly> GetNextBusinessDayAsync(DateOnly date, CancellationToken ct = default)
    {
        var next = date.AddDays(1);
        while (!await IsBusinessDayAsync(next, ct))
        {
            next = next.AddDays(1);
        }
        return next;
    }

    public async Task<DateOnly> GetPreviousBusinessDayAsync(DateOnly date, CancellationToken ct = default)
    {
        var prev = date.AddDays(-1);
        while (!await IsBusinessDayAsync(prev, ct))
        {
            prev = prev.AddDays(-1);
        }
        return prev;
    }

    public async Task<bool> IsBusinessDayAsync(DateOnly date, CancellationToken ct = default)
    {
        // 土日判定
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return false;
        }

        // 祝日判定
        if (await IsHolidayAsync(date, ct))
        {
            return false;
        }

        return true;
    }

    public async Task<bool> IsHolidayAsync(DateOnly date, CancellationToken ct = default)
    {
        // カレンダーマスタ登録日を休日として扱う
        return await _context.ProcessingCalendars
            .AnyAsync(c => c.ProcessingDate == date, ct);
    }

    public async Task<DateOnly> AdjustToBusinessDayAsync(DateOnly date, bool forward = true, CancellationToken ct = default)
    {
        while (!await IsBusinessDayAsync(date, ct))
        {
            date = forward ? date.AddDays(1) : date.AddDays(-1);
        }
        return date;
    }

    /// <inheritdoc />
    /// <remarks>
    /// ZGNS03 (ZGN940 呼出サブルーチン) 相当。
    /// TODO: 詳細設計フェーズで実装する。
    /// </remarks>
    /// <param name="date">日付を指定する。</param>
    /// <param name="businessDays">業務日一覧を指定する。</param>
    /// <param name="ct">キャンセルトークンを指定する。</param>
    public Task<DateOnly> AddBusinessDaysAsync(DateOnly date, int businessDays, CancellationToken ct = default)
    {
        throw new NotImplementedException("AddBusinessDaysAsync は詳細設計フェーズで実装予定 (ZGNS03 相当)。");
    }
}


