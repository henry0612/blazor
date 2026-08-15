using FluentAssertions;
using UnifiedAccount.Domain.Enums;

namespace UnifiedAccount.Domain.Tests;

public class TransferRoundTests
{
    /// <summary>このテストでは 振替回状態を更新したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void TransferRound_ShouldHaveExpectedNumericValues()
    {
        ((int)TransferRound.Round1).Should().Be(1);
        ((int)TransferRound.Round2).Should().Be(2);
        ((int)TransferRound.Round3).Should().Be(3);
    }

    /// <summary>このテストでは 振替回状態を更新したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void TransferRound_ShouldDefineThreeRoundsOnly()
    {
        Enum.GetValues<TransferRound>()
            .Should()
            .BeEquivalentTo([TransferRound.Round1, TransferRound.Round2, TransferRound.Round3]);
    }
}
