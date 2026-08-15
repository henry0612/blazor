using FluentAssertions;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Infrastructure.Services;
using UnifiedAccount.Integration.Tests.Helpers;

namespace UnifiedAccount.Integration.Tests.Services;

public class CalendarServiceTests
{
    /// <summary>このテストでは 平日の日付を渡したとき、営業日として判定される。</summary>
    [Fact]
    public async Task IsBusinessDayAsync_Weekday_ShouldReturnTrue()
    {
        // Arrange — 2026-03-16 is Monday
        using var db = TestDbContextFactory.Create();
        var service = new CalendarService(db);
        var monday = new DateOnly(2026, 3, 16);

        // Act
        var result = await service.IsBusinessDayAsync(monday);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>このテストでは 週末の日付を渡したとき、営業日ではないと判定される。</summary>
    [Theory]
    [InlineData(2026, 3, 14)] // Saturday
    [InlineData(2026, 3, 15)] // Sunday
    public async Task IsBusinessDayAsync_Weekend_ShouldReturnFalse(int y, int m, int d)
    {
        using var db = TestDbContextFactory.Create();
        var service = new CalendarService(db);

        var result = await service.IsBusinessDayAsync(new DateOnly(y, m, d));

        result.Should().BeFalse();
    }

    /// <summary>このテストでは 処理カレンダーに登録した日付を渡したとき、失敗する。</summary>
    [Fact]
    public async Task IsBusinessDayAsync_DateInProcessingCalendar_ShouldReturnFalse()
    {
        using var db = TestDbContextFactory.Create();
        db.ProcessingCalendars.Add(new ProcessingCalendar
        {
            ProcessingDate = new DateOnly(2026, 1, 1)
        });
        await db.SaveChangesAsync();

        var service = new CalendarService(db);

        var result = await service.IsBusinessDayAsync(new DateOnly(2026, 1, 1));

        result.Should().BeFalse();
    }

    /// <summary>このテストでは 金曜日を渡したとき、次の営業日が返される。</summary>
    [Fact]
    public async Task GetNextBusinessDayAsync_FromFriday_ShouldReturnMonday()
    {
        // 2026-03-13 is Friday → next business day is Monday 2026-03-16
        using var db = TestDbContextFactory.Create();
        var service = new CalendarService(db);

        var result = await service.GetNextBusinessDayAsync(new DateOnly(2026, 3, 13));

        result.Should().Be(new DateOnly(2026, 3, 16));
    }

    /// <summary>このテストでは 月曜日を渡したとき、前の営業日が返される。</summary>
    [Fact]
    public async Task GetPreviousBusinessDayAsync_FromMonday_ShouldReturnFriday()
    {
        // 2026-03-16 is Monday → previous business day is Friday 2026-03-13
        using var db = TestDbContextFactory.Create();
        var service = new CalendarService(db);

        var result = await service.GetPreviousBusinessDayAsync(new DateOnly(2026, 3, 16));

        result.Should().Be(new DateOnly(2026, 3, 13));
    }

    /// <summary>このテストでは 週末の日付を渡したとき、営業日ではないと判定される。</summary>
    [Fact]
    public async Task AdjustToBusinessDayAsync_OnWeekend_Forward_ShouldReturnMonday()
    {
        using var db = TestDbContextFactory.Create();
        var service = new CalendarService(db);
        var saturday = new DateOnly(2026, 3, 14);

        var result = await service.AdjustToBusinessDayAsync(saturday, forward: true);

        result.Should().Be(new DateOnly(2026, 3, 16));
    }

    /// <summary>このテストでは 処理カレンダーに登録した日付を渡したとき、成功する。</summary>
    [Fact]
    public async Task IsHolidayAsync_DateInProcessingCalendar_ShouldReturnTrue()
    {
        // ProcessingCalendar に登録済みの日付は休日とみなす
        using var db = TestDbContextFactory.Create();
        db.ProcessingCalendars.Add(new ProcessingCalendar
        {
            ProcessingDate = new DateOnly(2026, 1, 1)
        });
        await db.SaveChangesAsync();

        var service = new CalendarService(db);

        var result = await service.IsHolidayAsync(new DateOnly(2026, 1, 1));

        result.Should().BeTrue();
    }
}










