using System.Diagnostics;
using System.Security.Claims;
using FluentAssertions;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Web.Components.Pages.Code;

namespace UnifiedAccount.Integration.Tests.Ui;

public class CodeAuditLogEntryFactoryTests
{
    [Fact]
    public void CreateSearchEntry_UsesAuthenticatedActorAndCodeSettingTarget()
    {
        using var activity = new Activity("code-search").Start();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "admin01")],
            "test"));

        var entry = CodeAuditLogEntryFactory.CreateSearchEntry(principal, "BANK", "000");

        entry.ActorId.Should().Be("admin01");
        entry.FeatureId.Should().Be("F-ONL-100");
        entry.Action.Should().Be("Search");
        entry.TargetType.Should().Be("CodeSetting");
        entry.TargetKey.Should().Be("{\"CodeCategory\":\"BANK\",\"CodeValue\":\"000\"}");
        entry.Result.Should().Be("Success");
        entry.AfterValuesJson.Should().Be("{}");
        entry.CorrelationId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CreateSaveEntry_UsesSavedCodeAsTarget()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "admin01")],
            "test"));

        var entry = CodeAuditLogEntryFactory.CreateSaveEntry(principal, "BANK", "0001");

        entry.Action.Should().Be("Save");
        entry.TargetKey.Should().Be("{\"CodeCategory\":\"BANK\",\"CodeValue\":\"0001\"}");
        entry.BeforeValuesJson.Should().Be("{}");
        entry.AfterValuesJson.Should().Be("{}");
    }
}
