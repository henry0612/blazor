using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UnifiedAccount.Application.Services.Reports;
using UnifiedAccount.Batch.Framework;
using UnifiedAccount.Batch.Jobs.Report;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Integration.Tests.Batch;

public class SampleReportTests
{
    [Fact]
    public void SampleDetailTemplate_ShouldBindRequestTypeField()
    {
        var template = File.ReadAllText(GetRepositoryPath("open-system\\program\\reports\\sample-detail.dcx"));

        template.Should().Contain("CompanyCode,RequestType,ChangeAction");
        template.Should().Contain("<Field index=\"0\" valueAs=\"DATA\" valueType=\"SOURCE\">RequestType</Field>");
        template.Should().NotContain("DenkCode");
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCallReportOutputService()
    {
        var jobExecutionId = $"JOB-SampleReport-{DateTime.Now:yyyyMMdd}-000001";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"sample_report_test_{Guid.NewGuid():N}")
            .Options;

        await using var db = new AppDbContext(options);
        db.CompanyMasterChangeLogs.Add(new CompanyMasterChangeLog
        {
            JobExecutionId = jobExecutionId,
            ProcessingDate = new DateOnly(2026, 7, 1),
            CompanyCode = "000001",
            RequestType = "11",
            ChangeAction = "2",
            MessageType = 1,
            IsError = false,
            SequenceNo = 1,
            CompanyNameKana = "テスト会社"
        });
        await db.SaveChangesAsync();

        var reportServiceMock = new Mock<IReportOutputService>();
        reportServiceMock
            .Setup(x => x.OutputAsync(It.IsAny<ReportOutputRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReportOutputResponse
            {
                IsSuccessful = true,
                OutputFilePath = @"C:\temp\sample.pdf"
            });

        var filePathProviderMock = new Mock<IReportFilePathProvider>();
        filePathProviderMock
            .Setup(x => x.GetOutputFilePath(jobExecutionId, "SampleReport"))
            .Returns(@"C:\temp\sample.pdf");

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(db);
        services.AddSingleton(reportServiceMock.Object);
        services.AddSingleton(filePathProviderMock.Object);
        var provider = services.BuildServiceProvider();

        var context = new JobContext(
            processDate: new DateOnly(2026, 7, 1),
            services: provider,
            jobExecutionId: jobExecutionId);

        var transactionCoordinator = new TransactionCoordinator(NullLogger<TransactionCoordinator>.Instance);
        var sut = new SampleReport(transactionCoordinator, NullLoggerFactory.Instance, Mock.Of<IJobRunner>());

        var result = await sut.ExecuteAsync(context, CancellationToken.None);

        result.Success.Should().BeTrue();
        reportServiceMock.Verify(
            x => x.OutputAsync(
                It.Is<ReportOutputRequest>(r =>
                    r.ExecutionId == $"{jobExecutionId}-SAMPLE" &&
                    r.JobExecutionId == jobExecutionId &&
                    r.TemplateId == "SAMPLE" &&
                    r.DataRequest!.Mode == ReportDataMode.Streamed &&
                    r.OutputFormat == "PDF"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        reportServiceMock.Verify(
            x => x.OutputAsync(
                It.Is<ReportOutputRequest>(r =>
                    r.ExecutionId == $"{jobExecutionId}-SAMPLE-DETAIL" &&
                    r.JobExecutionId == jobExecutionId &&
                    r.TemplateId == "SAMPLE-DETAIL"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static string GetRepositoryPath(string relativePath)
    {
        var dir = AppContext.BaseDirectory;

        while (dir is not null && !File.Exists(Path.Combine(dir, "open-system", "program", "UnifiedAccount.slnx")))
        {
            dir = Directory.GetParent(dir)?.FullName;
        }

        dir.Should().NotBeNull();
        return Path.Combine(dir!, relativePath);
    }
}






