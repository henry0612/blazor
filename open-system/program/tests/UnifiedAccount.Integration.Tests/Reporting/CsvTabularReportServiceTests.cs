using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Reporting.Extensions;
using UnifiedAccount.Reporting.Models;
using UnifiedAccount.Reporting.Services;

namespace UnifiedAccount.Integration.Tests.Reporting;

/// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
/// CsvTabularReportService (エンジン不要の CSV フォールバック実装) のテスト
/// </summary>
public class CsvTabularReportServiceTests : IDisposable
{
    private readonly string _outputDir;
    private readonly ITabularReportService _service;

    public CsvTabularReportServiceTests()
    {
        _outputDir = Path.Combine(Path.GetTempPath(), $"csv_reporting_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_outputDir);

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug());
        // AddReportingCore() のみ — PDF プロバイダなし
        services.AddReportingCore();
        _service = services.BuildServiceProvider().GetRequiredService<ITabularReportService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
            Directory.Delete(_outputDir, true);
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task RenderAsync_CsvOutput_CreatesValidFile()
    {
        var outputPath = Path.Combine(_outputDir, "output.csv");
        var definition = CreateDefinition();

        var result = await _service.RenderAsync(definition, outputPath);

        result.Format.Should().Be("csv");
        result.RowCount.Should().Be(2);
        result.OutputPath.Should().Be(outputPath);
        File.Exists(outputPath).Should().BeTrue();

        var lines = await File.ReadAllLinesAsync(outputPath);
        lines.Should().HaveCount(3); // header + 2 rows
        lines[0].Should().Contain("会社コード");
        lines[1].Should().Contain("000001");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task RenderAsync_CsvWithSummaryRows_AppendsSummaryLines()
    {
        var outputPath = Path.Combine(_outputDir, "summary.csv");
        var definition = CreateDefinition() with
        {
            SummaryRows =
            [
                new Dictionary<string, object?>
                {
                    ["CompanyCode"] = "合計",
                    ["CompanyName"] = string.Empty,
                    ["Amount"] = 3000m
                }
            ]
        };

        var result = await _service.RenderAsync(definition, outputPath);

        result.RowCount.Should().Be(2);
        var lines = await File.ReadAllLinesAsync(outputPath);
        lines.Should().HaveCount(4); // header + 2 rows + 1 summary
        lines[3].Should().Contain("合計");
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task RenderAsync_PdfOutput_ThrowsNotSupportedException()
    {
        var outputPath = Path.Combine(_outputDir, "output.pdf");
        var definition = CreateDefinition();

        var act = () => _service.RenderAsync(definition, outputPath);

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*シーオーリポーツ帳票クリエータ*");
    }

    /// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
    [Fact]
    public void DI_AddReportingCore_ResolvesCsvService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddReportingCore();

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<ITabularReportService>();

        resolved.Should().BeOfType<CsvTabularReportService>();
    }

    /// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
    [Fact]
    public void DI_AddReportingServices_ResolvesCoReportsService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddReportingServices();         // AddReportingCore + CoReports プロバイダ

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<ITabularReportService>();

        // PDF プロバイダで上書きされていること
        resolved.Should().BeOfType<UnifiedAccount.Reporting.Services.CoReportsTabularReportService>();
    }

    private static TabularReportDefinition CreateDefinition() =>
        new TabularReportDefinition(
            "TEST_CSV",
            "CSVテスト帳票",
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
                    ["Amount"] = 2000m
                }
            ]);
}










