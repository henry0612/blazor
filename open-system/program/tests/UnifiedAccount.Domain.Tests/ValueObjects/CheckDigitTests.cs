using FluentAssertions;
using UnifiedAccount.Domain.ValueObjects;

namespace UnifiedAccount.Domain.Tests.ValueObjects;

public class CheckDigitTests
{
    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("0143", "001", "0143001")]
    [InlineData("0001", "001", "0001001")]
    [InlineData("9999", "999", "9999999")]
    public void BankBranchCode_ShouldCombineCorrectly(string bankCode, string branchCode, string expected)
    {
        var code = new BankBranchCode(bankCode, branchCode);
        code.ToString().Should().Be(expected);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("0143", "001")]
    [InlineData("0001", "001")]
    public void BankBranchCode_Equality_ShouldWork(string bankCode, string branchCode)
    {
        var code1 = new BankBranchCode(bankCode, branchCode);
        var code2 = new BankBranchCode(bankCode, branchCode);
        code1.Should().Be(code2);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("0143", "001", "0143", "002")]
    public void BankBranchCode_Different_ShouldNotBeEqual(string b1, string br1, string b2, string br2)
    {
        var code1 = new BankBranchCode(b1, br1);
        var code2 = new BankBranchCode(b2, br2);
        code1.Should().NotBe(code2);
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Theory]
    [InlineData("1000000")]
    [InlineData("0000001")]
    public void MoneyAmount_ShouldStoreValue(string amountStr)
    {
        var amount = decimal.Parse(amountStr);
        var money = new MoneyAmount(amount);
        money.Value.Should().Be(amount);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void MoneyAmount_Negative_ShouldThrow()
    {
        Action act = () => new MoneyAmount(-100m);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void MoneyAmount_ShouldSupportArithmetic()
    {
        var a = new MoneyAmount(100m);
        var b = new MoneyAmount(50m);
        (a + b).Value.Should().Be(150m);
        (a - b).Value.Should().Be(50m);
    }
}










