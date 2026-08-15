using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Web.Api;

/// <summary>
/// カレンダー API (KOZGUCL → /api/calendars) — 6エンドポイント
/// </summary>
public static class CalendarEndpoints
{
    public static RouteGroupBuilder MapCalendarEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetByMonth).WithName("GetCalendars");
        group.MapGet("/{date}", GetByDate).WithName("GetCalendarByDate");
        group.MapPut("/{date}", Update).WithValidation<UpdateCalendarRequest>().WithName("UpdateCalendar");
        group.MapPost("/", Create).WithValidation<CreateCalendarRequest>().WithName("CreateCalendar");
        group.MapPost("/copy", CopyToNextMonth).WithValidation<CopyCalendarRequest>().WithName("CopyCalendar");
        group.MapPost("/template", ApplyTemplate).WithValidation<ApplyTemplateRequest>().WithName("ApplyCalendarTemplate");
        return group;
    }

    private static async Task<IResult> GetByMonth(
        int year, int month, AppDbContext db, CancellationToken ct)
    {
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var calendars = await db.ProcessingCalendars
            .AsNoTracking()
            .Include(c => c.Slots)
            .Where(c => c.ProcessingDate >= startDate && c.ProcessingDate <= endDate)
            .OrderBy(c => c.ProcessingDate)
            .Select(c => new
            {
                c.Id,
                c.ProcessingDate,
                Slots = c.Slots.Select(s => new
                {
                    s.SlotNo,
                    s.ProcessingType,
                    s.WithdrawalDate1,
                    s.CompletedFlag
                }).OrderBy(s => s.SlotNo).ToList()
            })
            .ToListAsync(ct);

        return Results.Ok(calendars);
    }

    private static async Task<IResult> GetByDate(
        DateOnly date, AppDbContext db, CancellationToken ct)
    {
        var calendar = await db.ProcessingCalendars
            .AsNoTracking()
            .Include(c => c.Slots)
            .FirstOrDefaultAsync(c => c.ProcessingDate == date, ct);

        return calendar is null ? Results.NotFound() : Results.Ok(calendar);
    }

    /// <summary>
    /// UpdateCalendarRequest を表すレコード。
    /// </summary>
    public record UpdateCalendarRequest(string? ProcessingType);

    private static async Task<IResult> Update(
        DateOnly date, UpdateCalendarRequest req, AppDbContext db, CancellationToken ct)
    {
        var calendar = await db.ProcessingCalendars
            .Include(c => c.Slots)
            .FirstOrDefaultAsync(c => c.ProcessingDate == date, ct);

        if (calendar is null) return Results.NotFound();

        // 最初のスロットの処理種別を更新
        if (req.ProcessingType is not null)
        {
            var slot = calendar.Slots.FirstOrDefault();
            if (slot is not null)
                slot.ProcessingType = req.ProcessingType;
        }

        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    /// <summary>
    /// CreateCalendarRequest を表すレコード。
    /// </summary>
    public record CreateCalendarRequest(DateOnly ProcessingDate);

    private static async Task<IResult> Create(
        CreateCalendarRequest req, AppDbContext db, CancellationToken ct)
    {
        if (await db.ProcessingCalendars.AnyAsync(c => c.ProcessingDate == req.ProcessingDate, ct))
            return Results.Conflict(new { message = $"日付 '{req.ProcessingDate}' のカレンダーは既に登録されています。" });

        var calendar = new Domain.Entities.Core.ProcessingCalendar
        {
            ProcessingDate = req.ProcessingDate
        };
        db.ProcessingCalendars.Add(calendar);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/calendars/{calendar.ProcessingDate}", calendar);
    }

    /// <summary>
    /// CopyCalendarRequest を表すレコード。
    /// </summary>
    public record CopyCalendarRequest(int SourceYear, int SourceMonth);

    private static async Task<IResult> CopyToNextMonth(
        CopyCalendarRequest req, AppDbContext db, CancellationToken ct)
    {
        var sourceStart = new DateOnly(req.SourceYear, req.SourceMonth, 1);
        var sourceEnd = sourceStart.AddMonths(1).AddDays(-1);
        var targetStart = sourceStart.AddMonths(1);

        var sourceCalendars = await db.ProcessingCalendars
            .AsNoTracking()
            .Where(c => c.ProcessingDate >= sourceStart && c.ProcessingDate <= sourceEnd)
            .ToListAsync(ct);

        if (sourceCalendars.Count == 0)
            return Results.NotFound(new { message = "コピー元のカレンダーが見つかりません。" });

        int created = 0;
        foreach (var source in sourceCalendars)
        {
            var targetDate = targetStart.AddDays(source.ProcessingDate.Day - 1);
            if (targetDate.Month != targetStart.Month) continue; // 月末超過スキップ

            if (await db.ProcessingCalendars.AnyAsync(c => c.ProcessingDate == targetDate, ct))
                continue; // 既存はスキップ

            db.ProcessingCalendars.Add(new Domain.Entities.Core.ProcessingCalendar
            {
                ProcessingDate = targetDate
            });
            created++;
        }

        await db.SaveChangesAsync(ct);
        return Results.Ok(new { created });
    }

    /// <summary>
    /// ApplyTemplateRequest を表すレコード。
    /// </summary>
    public record ApplyTemplateRequest(int Year, int Month, string TemplateType);

    private static async Task<IResult> ApplyTemplate(
        ApplyTemplateRequest req, AppDbContext db, CancellationToken ct)
    {
        var start = new DateOnly(req.Year, req.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(req.Year, req.Month);
        int created = 0;

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(req.Year, req.Month, day);
            if (await db.ProcessingCalendars.AnyAsync(c => c.ProcessingDate == date, ct))
                continue;

            db.ProcessingCalendars.Add(new Domain.Entities.Core.ProcessingCalendar
            {
                ProcessingDate = date
            });
            created++;
        }

        await db.SaveChangesAsync(ct);
        return Results.Ok(new { created, templateType = req.TemplateType });
    }
}


