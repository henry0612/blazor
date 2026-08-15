using FluentAssertions;
using UnifiedAccount.Infrastructure.Services;

namespace UnifiedAccount.Integration.Tests.Services;

public class CheckDigitValidatorTests
{
    private readonly CheckDigitValidator _validator = new();

    /// <summary>このテストでは 会社コードと個人コードから期待どおりのチェックデジットを算出する。</summary>
    [Theory]
    [InlineData("TEST01", "012345", "3")]
    public void Calculate_ShouldReturnExpectedDigit(string companyCode, string personalCode, string expected)
    {
        var result = _validator.Calculate(companyCode, personalCode);

        result.Should().Be(expected);
    }

    /// <summary>このテストでは 算出したチェックデジットを与えると検証に成功する。</summary>
    [Theory]
    [InlineData("TEST01", "012345")]
    public void Validate_ShouldAcceptMatchingDigit(string companyCode, string personalCode)
    {
        var digit = _validator.Calculate(companyCode, personalCode);

        var isValid = _validator.Validate(companyCode, personalCode, digit);

        isValid.Should().BeTrue();
    }

    /// <summary>このテストでは 異なるASCII数字1桁を与えると検証に失敗する。</summary>
    [Theory]
    [InlineData("TEST01", "012345", "9")]
    public void Validate_ShouldRejectIncorrectDigit(string companyCode, string personalCode, string wrongDigit)
    {
        var isValid = _validator.Validate(companyCode, personalCode, wrongDigit);

        isValid.Should().BeFalse();
    }

    /// <summary>このテストでは checkDigit がASCII数字1桁以外のとき ArgumentException となる。</summary>
    [Theory]
    [InlineData("X")]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("１")]
    public void Validate_InvalidCheckDigit_ShouldThrowArgumentException(string invalidCheckDigit)
    {
        var act = () => _validator.Validate("TEST01", "012345", invalidCheckDigit);

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("checkDigit");
    }

    /// <summary>このテストでは checkDigit が null のとき ArgumentException となる。</summary>
    [Fact]
    public void Validate_NullCheckDigit_ShouldThrowArgumentException()
    {
        var act = () => _validator.Validate("TEST01", "012345", null!);

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("checkDigit");
    }

    /// <summary>このテストでは Mod10 検証が一致時に true を返す。</summary>
    [Fact]
    public void ValidateMod10_MatchingDigit_ShouldReturnTrue()
    {
        _validator.ValidateMod10("1234567", "4").Should().BeTrue();
    }

    /// <summary>このテストでは Mod10 検証が不一致時に false を返す。</summary>
    [Fact]
    public void ValidateMod10_NotMatchingDigit_ShouldReturnFalse()
    {
        _validator.ValidateMod10("1234567", "3").Should().BeFalse();
    }

    /// <summary>このテストでは Mod11 検証の checkDigit が不正なとき ArgumentException となる。</summary>
    [Fact]
    public void ValidateMod11_InvalidCheckDigit_ShouldThrowArgumentException()
    {
        var act = () => _validator.ValidateMod11("1234567", "X");

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("checkDigit");
    }

    /// <summary>このテストでは accountNumber が不正なとき ArgumentException となる。</summary>
    [Fact]
    public void ValidateMod10_InvalidAccountNumber_ShouldThrowArgumentException()
    {
        var act = () => _validator.ValidateMod10("A01", "4");

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("accountNumber");
    }
}





