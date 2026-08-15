using FluentAssertions;
using UnifiedAccount.Domain.Enums;
using UnifiedAccount.Domain.Services;

namespace UnifiedAccount.Domain.Tests.Entities;

public class ChangeRequestDestinationRouterTests
{
    [Theory]
    [InlineData("01", null, ChangeRequestDestination.Bank)]
    [InlineData("11", null, ChangeRequestDestination.Company)]
    [InlineData("19", null, ChangeRequestDestination.Company)]
    [InlineData("20", null, ChangeRequestDestination.AccountNumber)]
    [InlineData("21", null, ChangeRequestDestination.Contract)]
    [InlineData("24", null, ChangeRequestDestination.Contract)]
    [InlineData("71", "1", ChangeRequestDestination.Company)]
    [InlineData("71", "3", ChangeRequestDestination.Company)]
    [InlineData("72", "01", ChangeRequestDestination.Message)]
    [InlineData("72", "13", ChangeRequestDestination.Message)]
    public void Resolve_ShouldRouteEveryIf01Voucher(string requestType, string? cban, ChangeRequestDestination expected)
    {
        ChangeRequestDestinationRouter.Resolve(requestType, cban).Should().Be(expected);
    }

    [Theory]
    [InlineData("30", null)]
    [InlineData("40", null)]
    [InlineData("71", "4")]
    [InlineData("72", "14")]
    public void Resolve_ShouldRejectUnsupportedVoucher(string requestType, string? cban)
    {
        ChangeRequestDestinationRouter.Resolve(requestType, cban)
            .Should()
            .Be(ChangeRequestDestination.Unsupported);
    }
}
