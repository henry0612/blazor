using FluentAssertions;
using UnifiedAccount.Domain.ValueObjects;

namespace UnifiedAccount.Domain.Tests.ValueObjects;

public class ProcessingDateTests
{
    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void ValidDateOnly_ShouldCreate()
    {
        var date = new ProcessingDate(new DateOnly(2026, 3, 16));
        date.Value.Should().Be(new DateOnly(2026, 3, 16));
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void ValidYearMonthDay_ShouldCreate()
    {
        var date = new ProcessingDate(2026, 1, 15);
        date.Value.Should().Be(new DateOnly(2026, 1, 15));
    }

    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void DefaultDateOnly_ShouldThrow()
    {
        Action act = () => new ProcessingDate(default(DateOnly));
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void Equality_SameDate_ShouldBeEqual()
    {
        var date1 = new ProcessingDate(2026, 3, 16);
        var date2 = new ProcessingDate(new DateOnly(2026, 3, 16));
        date1.Should().Be(date2);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void Equality_DifferentDates_ShouldNotBeEqual()
    {
        var date1 = new ProcessingDate(2026, 3, 16);
        var date2 = new ProcessingDate(2026, 3, 17);
        date1.Should().NotBe(date2);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void ImplicitConversion_ShouldReturnDateOnly()
    {
        var processingDate = new ProcessingDate(2026, 6, 1);
        DateOnly value = processingDate;
        value.Should().Be(new DateOnly(2026, 6, 1));
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void ToString_ShouldFormatAsIso()
    {
        var date = new ProcessingDate(2026, 3, 16);
        date.ToString().Should().Be("2026-03-16");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void LeapYearDate_ShouldCreate()
    {
        var date = new ProcessingDate(2024, 2, 29);
        date.Value.Should().Be(new DateOnly(2024, 2, 29));
    }
}










