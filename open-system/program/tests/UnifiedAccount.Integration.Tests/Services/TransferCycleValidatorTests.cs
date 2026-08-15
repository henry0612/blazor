using FluentAssertions;
using UnifiedAccount.Domain.Enums;
using UnifiedAccount.Infrastructure.Services;

namespace UnifiedAccount.Integration.Tests.Services;

public class TransferCycleValidatorTests
{
    private readonly TransferCycleValidator _validator = new();

    /// <summary>このテストでは 振替回状態を更新したとき、成功する。</summary>
    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    [InlineData("")]
    [InlineData(null)]
    public void IsValid_Round1_ShouldAlwaysReturnTrue(string? previousRound)
    {
        var result = _validator.IsValid(TransferRound.Round1, previousRound);

        result.Should().BeTrue();
    }

    /// <summary>このテストでは 振替回状態を更新したとき、成功する。</summary>
    [Theory]
    [InlineData("0")]
    [InlineData("3")]
    [InlineData(null)]
    [InlineData("")]
    public void IsValid_Round2_WithAllowedPreviousRound_ShouldReturnTrue(string? previousRound)
    {
        var result = _validator.IsValid(TransferRound.Round2, previousRound);

        result.Should().BeTrue();
    }

    /// <summary>このテストでは 振替回状態を更新したとき、失敗する。</summary>
    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("9")]
    public void IsValid_Round2_WithDisallowedPreviousRound_ShouldReturnFalse(string previousRound)
    {
        var result = _validator.IsValid(TransferRound.Round2, previousRound);

        result.Should().BeFalse();
    }

    /// <summary>このテストでは 振替回状態を更新したとき、成功する。</summary>
    [Fact]
    public void IsValid_Round3_WithPreviousRound2_ShouldReturnTrue()
    {
        var result = _validator.IsValid(TransferRound.Round3, "2");

        result.Should().BeTrue();
    }

    /// <summary>このテストでは 振替回状態を更新したとき、失敗する。</summary>
    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("3")]
    [InlineData(null)]
    [InlineData("")]
    public void IsValid_Round3_WithDisallowedPreviousRound_ShouldReturnFalse(string? previousRound)
    {
        var result = _validator.IsValid(TransferRound.Round3, previousRound);

        result.Should().BeFalse();
    }

    /// <summary>このテストでは 振替回状態を更新したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void IsValid_WithUndefinedRound_ShouldThrow()
    {
        var act = () => _validator.IsValid((TransferRound)99, "0");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>このテストでは 振替サイクルを検証したとき、サイクル検証の結果が返される。</summary>
    [Fact]
    public void EnsureValid_WithInvalidCycle_ShouldThrowDetailedException()
    {
        var act = () => _validator.EnsureValid(TransferRound.Round3, "0", "TEST01");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Company=TEST01*Round=3*Previous=0*");
    }

    /// <summary>このテストでは 振替サイクルを検証したとき、サイクル検証の結果が返される。</summary>
    [Fact]
    public void EnsureValid_WithValidCycle_ShouldNotThrow()
    {
        var act = () => _validator.EnsureValid(TransferRound.Round2, "3", "TEST01");

        act.Should().NotThrow();
    }
}
