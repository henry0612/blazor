using FluentAssertions;
using UnifiedAccount.Domain.Enums;
using UnifiedAccount.Domain.Services;

namespace UnifiedAccount.Domain.Tests.Entities;

public class CompanyChangeRequestRoutingTests
{
    [Theory]
    [InlineData("11", null, CompanyChangeRequestRoute.CompanyBasic11)]
    [InlineData("12", null, CompanyChangeRequestRoute.CompanyAddress12)]
    [InlineData("13", null, CompanyChangeRequestRoute.CompanyProcessing13)]
    [InlineData("14", null, CompanyChangeRequestRoute.CompanyFee14)]
    [InlineData("15", null, CompanyChangeRequestRoute.CompanyTransfer15)]
    [InlineData("16", null, CompanyChangeRequestRoute.CompanyType16To19)]
    [InlineData("19", null, CompanyChangeRequestRoute.CompanyType16To19)]
    [InlineData(" 71 ", " 1 ", CompanyChangeRequestRoute.CompanyKanjiName71)]
    [InlineData("71", "2", CompanyChangeRequestRoute.CompanyKanjiAddress71)]
    [InlineData("71", "3", CompanyChangeRequestRoute.CompanyKanjiContact71)]
    public void Resolve_ShouldReturnExpectedRoute(string requestType, string? cban, CompanyChangeRequestRoute expected)
    {
        CompanyChangeRequestRouter.Resolve(requestType, cban).Should().Be(expected);
    }

    [Theory]
    [InlineData("71", "9")]
    [InlineData("72", "1")]
    [InlineData("", null)]
    [InlineData("   ", " 1 ")]
    public void IsSupported_ShouldReturnFalse_ForUnsupportedPatterns(string requestType, string? cban)
    {
        CompanyChangeRequestRouter.IsSupported(requestType, cban).Should().BeFalse();
    }
}
