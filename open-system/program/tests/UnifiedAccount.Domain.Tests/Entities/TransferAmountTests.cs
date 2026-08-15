using FluentAssertions;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Domain.Tests.Entities;

public class TransferAmountTests
{
    [Fact]
    public void Amounts_AcceptNullAndZero()
    {
        var entity = new TransferAmount
        {
            Amount1 = null,
            Amount2 = 0m,
            Amount3 = null,
            Amount4 = 0m,
            Amount5 = null
        };

        entity.Amount1.Should().BeNull();
        entity.Amount2.Should().Be(0m);
        entity.Amount3.Should().BeNull();
        entity.Amount4.Should().Be(0m);
        entity.Amount5.Should().BeNull();
    }
}
