using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using UnifiedAccount.Batch.Framework;
using UnifiedAccount.Batch.Jobs.Report;
using UnifiedAccount.Batch.Workflows;

namespace UnifiedAccount.Integration.Tests.Workflows;

public class SampleWorkflowTests
{
    /// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
    [Fact]
    public async Task ExecuteAsync_SingleReport_ShouldCallSampleReport()
    {
        var runner = new RecordingJobRunner();
        var transactionCoordinator = new TransactionCoordinator(NullLogger<TransactionCoordinator>.Instance);
        var workflow = new SampleWorkflow(runner, transactionCoordinator, NullLoggerFactory.Instance);
        var services = new ServiceCollection().BuildServiceProvider();

        var ctx = new JobContext(new DateOnly(2026, 3, 17), services, new Dictionary<string, string>
        {
            ["ReportCode"] = "0001",
        });

        var result = await workflow.ExecuteAsync(ctx);

        result.Success.Should().BeTrue();
        runner.ExecutedJobTypes.Should().Contain(typeof(SampleReport));
        runner.ExecutedReportCodes.Should().Contain("0001");
    }

    /// <summary>このテストでは 帳票を生成したとき、失敗する。</summary>
    [Fact]
    public async Task ExecuteAsync_MultipleReports_OneFails_ShouldReturnPartialFailure()
    {
        var runner = new ConditionalFailingRunner();
        var transactionCoordinator = new TransactionCoordinator(NullLogger<TransactionCoordinator>.Instance);
        var workflow = new SampleWorkflow(runner, transactionCoordinator, NullLoggerFactory.Instance);
        var services = new ServiceCollection().BuildServiceProvider();

        var ctx = new JobContext(new DateOnly(2026, 3, 17), services, new Dictionary<string, string>
        {
            ["ReportCode"] = "0001,0006,0143",
        });

        var result = await workflow.ExecuteAsync(ctx);

        result.Success.Should().BeFalse();
        runner.ExecutedReportCodes.Should().Contain(new[] { "0001", "0006", "0143" });
        result.ErrorCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void ToJobResult_AllJobsSucceeded_ShouldReturnSuccessfulJobResult()
    {
        var workflowResult = new WorkflowResult(new[]
        {
            JobResult.Ok(readCount: 3, writeCount: 2, message: "first"),
            JobResult.Ok(readCount: 4, writeCount: 1, message: "second"),
        });

        var result = workflowResult.ToJobResult();

        result.Success.Should().BeTrue();
        result.ReadCount.Should().Be(7);
        result.WriteCount.Should().Be(3);
        result.ErrorCount.Should().Be(0);
        result.Message.Should().Contain("Jobs=2");
        result.Message.Should().Contain("OK=2");
    }

    [Fact]
    public void JobResult_Timestamps_ShouldUseLocalTime()
    {
        var result = JobResult.Ok();

        result.StartedAt.Kind.Should().Be(DateTimeKind.Local);
        result.CompletedAt.Should().NotBeNull();
        result.CompletedAt!.Value.Kind.Should().Be(DateTimeKind.Local);
    }

    [Fact]
    public void ToJobResult_AnyJobFailed_ShouldReturnFailureJobResult()
    {
        var workflowResult = new WorkflowResult(new[]
        {
            JobResult.Ok(readCount: 3, writeCount: 2, message: "first"),
            JobResult.Fail("boom", readCount: 2, errorCount: 3),
        });

        var result = workflowResult.ToJobResult();

        result.Success.Should().BeFalse();
        result.ReadCount.Should().Be(5);
        result.WriteCount.Should().Be(2);
        result.ErrorCount.Should().Be(3);
        result.Message.Should().Contain("NG=1");
        result.Message.Should().Contain("E=3");
    }

    private sealed class RecordingJobRunner : IJobRunner
    {
        public List<Type> ExecutedJobTypes { get; } = new();
        public List<string> ExecutedReportCodes { get; } = new();

        public Task<JobResult> RunAsync<TJob>(JobContext context, CancellationToken ct = default) where TJob : IJob
        {
            ExecutedJobTypes.Add(typeof(TJob));
            ExecutedReportCodes.Add(context.GetParameter("ReportCode", string.Empty));
            return Task.FromResult(JobResult.Ok(message: typeof(TJob).Name));
        }

        public Task<JobResult> RunAsync(string jobId, JobContext context, CancellationToken ct = default)
        {
            ExecutedJobTypes.Add(typeof(string));
            ExecutedReportCodes.Add(context.GetParameter("ReportCode", string.Empty));
            return Task.FromResult(JobResult.Ok(message: jobId));
        }
    }

    private sealed class ConditionalFailingRunner : IJobRunner
    {
        public List<string> ExecutedReportCodes { get; } = new();

        public Task<JobResult> RunAsync<TJob>(JobContext context, CancellationToken ct = default) where TJob : IJob
        {
            var reportCode = context.GetParameter("ReportCode", string.Empty);
            ExecutedReportCodes.Add(reportCode);
            if (reportCode == "0006")
                return Task.FromResult(JobResult.Fail("FEP error"));
            return Task.FromResult(JobResult.Ok(message: typeof(TJob).Name));
        }

        public Task<JobResult> RunAsync(string jobId, JobContext context, CancellationToken ct = default)
        {
            var reportCode = context.GetParameter("ReportCode", string.Empty);
            ExecutedReportCodes.Add(reportCode);
            if (reportCode == "0006")
                return Task.FromResult(JobResult.Fail("FEP error"));
            return Task.FromResult(JobResult.Ok(message: jobId));
        }
    }
}









