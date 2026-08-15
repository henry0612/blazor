using FluentAssertions;
using UnifiedAccount.Domain.ValueObjects;

namespace UnifiedAccount.Domain.Tests.ValueObjects;

public class ConsignorCodeTests
{
    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("1234567890")]
    [InlineData("0000000001")]
    public void ValidConsignorCode_ShouldCreate(string code)
    {
        var consignorCode = new ConsignorCode(code);
        consignorCode.Value.Should().Be(code);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyConsignorCode_ShouldThrow(string? code)
    {
        Action act = () => new ConsignorCode(code!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void ConsignorCode_Equality_ShouldWork()
    {
        var code1 = new ConsignorCode("1234567890");
        var code2 = new ConsignorCode("1234567890");
        code1.Should().Be(code2);
    }
}










