using FluentAssertions;
using UnifiedAccount.Domain.ValueObjects;

namespace UnifiedAccount.Domain.Tests.ValueObjects;

public class AccountNumberTests
{
    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("1234567")]
    [InlineData("0000001")]
    [InlineData("9999999")]
    public void ValidAccountNumber_ShouldCreate(string number)
    {
        var account = new AccountNumber(number);
        account.Value.Should().Be(number);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void EmptyOrWhitespace_ShouldThrow(string? number)
    {
        Action act = () => new AccountNumber(number!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("123456")]
    [InlineData("12345678")]
    public void WrongLength_ShouldThrow(string number)
    {
        Action act = () => new AccountNumber(number);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("ABCDEFG")]
    [InlineData("123456A")]
    [InlineData("12 4567")]
    public void NonDigit_ShouldThrow(string number)
    {
        Action act = () => new AccountNumber(number);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var num1 = new AccountNumber("1234567");
        var num2 = new AccountNumber("1234567");
        num1.Should().Be(num2);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void ImplicitConversion_ShouldReturnValue()
    {
        var account = new AccountNumber("1234567");
        string value = account;
        value.Should().Be("1234567");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var account = new AccountNumber("0000001");
        account.ToString().Should().Be("0000001");
    }
}










