using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Reporting.Extensions;
using UnifiedAccount.Reporting.Models;
using UnifiedAccount.Reporting.Services;

namespace UnifiedAccount.Integration.Tests.Reporting;

public class CoReportsTabularReportServiceTests : IDisposable
{
    private readonly string _outputDir;
    private readonly ITabularReportService _service;

    public CoReportsTabularReportServiceTests()
    {
        _outputDir = Path.Combine(Path.GetTempPath(), $"reporting_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_outputDir);

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug());
        services.AddReportingServices();
        _service = services.BuildServiceProvider().GetRequiredService<ITabularReportService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
            Directory.Delete(_outputDir, true);
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task RenderAsync_CsvOutput_ShouldCreateCsvFile()
    {
        var outputPath = Path.Combine(_outputDir, "sample.csv");
        var definition = CreateDefinition();

        var result = await _service.RenderAsync(definition, outputPath);

        result.Format.Should().Be("csv");
        result.RowCount.Should().Be(2);
        File.Exists(outputPath).Should().BeTrue();
        var lines = await File.ReadAllLinesAsync(outputPath);
        lines.Should().HaveCount(3);
        lines[0].Should().Contain("会社コード");
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task RenderAsync_PdfOutput_ShouldCreatePdfFile()
    {
        var outputPath = Path.Combine(_outputDir, "sample.pdf");
        var definition = CreateDefinition();

        var result = await _service.RenderAsync(definition, outputPath);

        result.Format.Should().Be("pdf");
        File.Exists(outputPath).Should().BeTrue();
        new FileInfo(outputPath).Length.Should().BeGreaterThan(0);
    }

    private static TabularReportDefinition CreateDefinition()
    {
        return new TabularReportDefinition(
            "TEST001",
            "テスト帳票",
            [
                new ReportColumn("CompanyCode", "会社コード", 80f),
                new ReportColumn("CompanyName", "会社名", 140f),
                new ReportColumn("Amount", "金額", 80f)
            ],
            [
                new Dictionary<string, object?>
                {
                    ["CompanyCode"] = "000001",
                    ["CompanyName"] = "テスト会社A",
                    ["Amount"] = 1000m
                },
                new Dictionary<string, object?>
                {
                    ["CompanyCode"] = "000002",
                    ["CompanyName"] = "テスト会社B",
                    ["Amount"] = 2500m
                }
            ]);
    }
}
