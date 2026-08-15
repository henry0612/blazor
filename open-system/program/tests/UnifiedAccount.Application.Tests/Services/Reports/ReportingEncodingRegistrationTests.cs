using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using UnifiedAccount.Reporting.Extensions;

namespace UnifiedAccount.Application.Tests.Services.Reports;

public class ReportingEncodingRegistrationTests
{
    /// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
    [Fact]
    public void AddReportingCore_RegistersCodePagesProvider()
    {
        ResetRegistrationState();

        var services = new ServiceCollection();
        services.AddReportingCore();

        GetRegistrationState().Should().BeTrue();
    }

    /// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
    [Fact]
    public void AddCoReportsProvider_RegistersCodePagesProvider()
    {
        ResetRegistrationState();

        var services = new ServiceCollection();
        services.AddCoReportsProvider();

        GetRegistrationState().Should().BeTrue();
    }

    private static void ResetRegistrationState()
    {
        var type = typeof(DependencyInjection).Assembly
            .GetType("UnifiedAccount.Reporting.Extensions.ReportingEncodingInitializer", throwOnError: false);
        type.Should().NotBeNull();

        var field = type!.GetField("_codePagesProviderRegistered", BindingFlags.NonPublic | BindingFlags.Static);
        field.Should().NotBeNull();
        field!.SetValue(null, false);
    }

    private static bool GetRegistrationState()
    {
        var type = typeof(DependencyInjection).Assembly
            .GetType("UnifiedAccount.Reporting.Extensions.ReportingEncodingInitializer", throwOnError: false);
        type.Should().NotBeNull();

        var field = type!.GetField("_codePagesProviderRegistered", BindingFlags.NonPublic | BindingFlags.Static);
        field.Should().NotBeNull();
        return (bool)field!.GetValue(null)!;
    }
}










