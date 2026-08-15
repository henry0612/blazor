using FluentAssertions;
using UnifiedAccount.Domain.ValueObjects;

namespace UnifiedAccount.Domain.Tests.ValueObjects;

public class CompanyCodeTests
{
    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("TEST01")]
    [InlineData("000001")]
    [InlineData("ABCDEF")]
    public void ValidCompanyCode_ShouldCreate(string code)
    {
        var companyCode = new CompanyCode(code);
        companyCode.Value.Should().Be(code);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void EmptyOrWhitespace_ShouldThrow(string? code)
    {
        Action act = () => new CompanyCode(code!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("SHORT")]
    [InlineData("TOOLONG7")]
    public void WrongLength_ShouldThrow(string code)
    {
        Action act = () => new CompanyCode(code);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var code1 = new CompanyCode("TEST01");
        var code2 = new CompanyCode("TEST01");
        code1.Should().Be(code2);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        var code1 = new CompanyCode("TEST01");
        var code2 = new CompanyCode("TEST02");
        code1.Should().NotBe(code2);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void ImplicitConversion_ShouldReturnValue()
    {
        var companyCode = new CompanyCode("TEST01");
        string value = companyCode;
        value.Should().Be("TEST01");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var companyCode = new CompanyCode("ABC123");
        companyCode.ToString().Should().Be("ABC123");
    }
}










