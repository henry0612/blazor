using FluentAssertions;
using UnifiedAccount.Application.Services;

namespace UnifiedAccount.Application.Tests.Services;

public class ExecutionNumberServiceTests
{
    /// <summary>このテストでは 実行結果を取得したとき、実行結果が記録される。</summary>
    [Fact]
    public void GenerateJobExecutionId_ShouldUseJobIdAndProcessDatePrefix()
    {
        var service = new ExecutionNumberService();

        var id = service.GenerateJobExecutionId("KOZ010", new DateOnly(2026, 6, 30));

        id.Should().StartWith("JOB-KOZ010-20260630-");
        id.Should().MatchRegex("^JOB-KOZ010-20260630-[A-Z0-9]{6}$");
    }

    /// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
    [Fact]
    public void GenerateReportDataId_ShouldUseTemplateIdAndProcessDatePrefix()
    {
        var service = new ExecutionNumberService();

        var id = service.GenerateReportDataId("REP-001", new DateOnly(2026, 6, 30));

        id.Should().StartWith("REPORT-REP-001-20260630-");
        id.Should().MatchRegex("^REPORT-REP-001-20260630-[A-Z0-9]{6}$");
    }
}










