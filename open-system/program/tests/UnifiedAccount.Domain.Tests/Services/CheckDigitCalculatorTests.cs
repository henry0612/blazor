using FluentAssertions;
using UnifiedAccount.Domain.Services;

namespace UnifiedAccount.Domain.Tests.Services;

public class CheckDigitCalculatorTests
{
    /// <summary>このテストでは 222 §6.1 の回帰テストベクタに対しMod10が期待値を返す。</summary>
    [Theory]
    [InlineData("1234567", "4")]
    [InlineData("0000000000", "0")]
    [InlineData("7992739871", "3")]
    [InlineData("0012345678", "2")]
    [InlineData("01012345", "3")]
    public void CalculateMod10_RegressionVectors_ShouldReturnExpectedDigit(string input, string expected)
    {
        CheckDigitCalculator.CalculateMod10(input).Should().Be(expected);
    }

    /// <summary>このテストでは 222 §6.1 の回帰テストベクタに対しMod11が期待値を返す。</summary>
    [Theory]
    [InlineData("1234567", "4")]
    [InlineData("0000000000", "0")]
    [InlineData("7992739871", "6")]
    [InlineData("0012345678", "5")]
    [InlineData("01012345", "3")]
    public void CalculateMod11_RegressionVectors_ShouldReturnExpectedDigit(string input, string expected)
    {
        CheckDigitCalculator.CalculateMod11(input).Should().Be(expected);
    }

    /// <summary>このテストでは 222 §4.2 の remainder が11となる入力に対し "0" を返す。</summary>
    [Fact]
    public void CalculateMod11_RemainderEleven_ShouldReturnZero()
    {
        CheckDigitCalculator.CalculateMod11("0000").Should().Be("0");
    }

    /// <summary>このテストでは 222 §4.2 の remainder が10となる入力に対し "0" を返す。</summary>
    [Fact]
    public void CalculateMod11_RemainderTen_ShouldReturnZero()
    {
        CheckDigitCalculator.CalculateMod11("0006").Should().Be("0");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void CalculateForCompanyAndPersonal_ShouldUseOnlyDigits()
    {
        var result = CheckDigitCalculator.CalculateForCompanyAndPersonal("TEST01", "012345");

        result.Should().Be("3");
    }

    /// <summary>
    /// 現行 COBOL（KOZ035N / KOZ830 の CHECK-DIGIT セクション）由来の回帰ベクタ。
    /// 会社コード CHAR(6) と個人コード CHAR(12) を連結した 18 桁へ Mod10 を適用する。
    /// COBOL は 18 桁固定位置へ左端から重み 1,2 を交互適用し、積が 2 桁なら各桁を加算し、
    /// 合計の 10 の補数を PIC 9(1) へ格納する（余り 0 のときは切り捨てで 0 になる）。
    /// </summary>
    [Theory]
    [InlineData("000001", "000001000001", "4")]
    [InlineData("000001", "000001000002", "2")]
    [InlineData("000001", "000001000003", "0")]
    [InlineData("000001", "000001000004", "8")]
    [InlineData("000001", "000001000005", "5")]
    [InlineData("000002", "000002000001", "0")]
    [InlineData("000003", "000003000001", "6")]
    public void CalculateForCompanyAndPersonal_CobolRegressionVectors_ShouldMatchLegacy(
        string companyCode, string personalCode, string expected)
    {
        CheckDigitCalculator.CalculateForCompanyAndPersonal(companyCode, personalCode)
            .Should().Be(expected);
    }

    /// <summary>
    /// 個人コード単独の Mod10 は現行 COBOL と一致しない。
    /// 会社コード 6 桁分の寄与が落ちるためである。非互換であることを明示的に固定する。
    /// </summary>
    [Fact]
    public void CalculateMod10_PersonalCodeOnly_ShouldNotMatchLegacy()
    {
        var legacy = CheckDigitCalculator.CalculateForCompanyAndPersonal("000001", "000001000001");
        var personalOnly = CheckDigitCalculator.CalculateMod10("000001000001");

        legacy.Should().Be("4");
        personalOnly.Should().Be("6");
        personalOnly.Should().NotBe(legacy);
    }

    /// <summary>このテストでは accountNumber が空のとき ArgumentException となる。</summary>
    [Fact]
    public void CalculateMod10_Empty_ShouldThrowArgumentException()
    {
        var act = () => CheckDigitCalculator.CalculateMod10("");

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("accountNumber");
    }

    /// <summary>このテストでは accountNumber が null のとき ArgumentException となる。</summary>
    [Fact]
    public void CalculateMod11_Null_ShouldThrowArgumentException()
    {
        var act = () => CheckDigitCalculator.CalculateMod11(null!);

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("accountNumber");
    }

    /// <summary>このテストでは accountNumber に英字が混在するとき ArgumentException となる。</summary>
    [Fact]
    public void CalculateMod10_WithAlphabet_ShouldThrowArgumentException()
    {
        var act = () => CheckDigitCalculator.CalculateMod10("A01");

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("accountNumber");
    }

    /// <summary>このテストでは accountNumber が全角数字のとき ArgumentException となる。</summary>
    [Fact]
    public void CalculateMod11_FullWidthDigits_ShouldThrowArgumentException()
    {
        var act = () => CheckDigitCalculator.CalculateMod11("１２３");

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("accountNumber");
    }

    /// <summary>このテストでは accountNumber が19桁のとき ArgumentException となる。</summary>
    [Fact]
    public void CalculateMod10_NineteenDigits_ShouldThrowArgumentException()
    {
        var act = () => CheckDigitCalculator.CalculateMod10("1234567890123456789");

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("accountNumber");
    }

    /// <summary>このテストでは連結値が全非数字のとき ArgumentException となる（長さフォールバックを行わない）。</summary>
    [Fact]
    public void CalculateForCompanyAndPersonal_AllNonDigits_ShouldThrowArgumentException()
    {
        var act = () => CheckDigitCalculator.CalculateForCompanyAndPersonal("AAA", "BBB");

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("companyCode");
    }

    /// <summary>このテストでは全角数字を計算対象から除外する（ASCII半角数字だけを抽出する）。</summary>
    [Fact]
    public void CalculateForCompanyAndPersonal_FullWidthDigitsOnly_ShouldThrowArgumentException()
    {
        var act = () => CheckDigitCalculator.CalculateForCompanyAndPersonal("１２３", "４５６");

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("companyCode");
    }

    /// <summary>このテストでは companyCode が空のとき ArgumentException となる。</summary>
    [Fact]
    public void CalculateForCompanyAndPersonal_EmptyCompanyCode_ShouldThrowArgumentException()
    {
        var act = () => CheckDigitCalculator.CalculateForCompanyAndPersonal("", "012345");

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("companyCode");
    }

    /// <summary>このテストでは personalCode が空のとき ArgumentException となる。</summary>
    [Fact]
    public void CalculateForCompanyAndPersonal_EmptyPersonalCode_ShouldThrowArgumentException()
    {
        var act = () => CheckDigitCalculator.CalculateForCompanyAndPersonal("TEST01", "");

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("personalCode");
    }
}
