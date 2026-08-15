using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Reporting.Extensions;
using UnifiedAccount.Reporting.Models;
using UnifiedAccount.Reporting.Services;

namespace UnifiedAccount.Integration.Tests.Reporting;

/// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
/// IFormReportService 実装 (PDF / CSV) のテスト
/// </summary>
public class FormReportServiceTests : IDisposable
{
    private readonly string _outputDir;
    private readonly IFormReportService _coReportsService;
    private readonly IFormReportService _csvService;

    public FormReportServiceTests()
    {
        _outputDir = Path.Combine(Path.GetTempPath(), $"form_report_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_outputDir);

        var logging = new ServiceCollection().AddLogging(b => b.AddDebug());

        // CoReports サービス
        var provider = logging.BuildServiceProvider();
        _coReportsService = new UnifiedAccount.Reporting.Services.CoReportsFormReportService(
            provider.GetRequiredService<ILogger<UnifiedAccount.Reporting.Services.CoReportsFormReportService>>());

        // CSV フォールバックサービス
        _csvService = new CsvFormReportService(
            provider.GetRequiredService<ILogger<CsvFormReportService>>());
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
            Directory.Delete(_outputDir, true);
    }

    // ------------------------------------------------------------------ PDF tests
    /// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
    [Fact]
    public async Task CoReports_CoverLetter_ShouldCreatePdfFile()
    {
        var outputPath = Path.Combine(_outputDir, "cover_letter.pdf");
        var definition = CreateCoverLetterDefinition();

        var result = await _coReportsService.RenderAsync(definition, outputPath);

        result.Format.Should().Be("pdf");
        result.PageCount.Should().Be(1);
        File.Exists(outputPath).Should().BeTrue();
        new FileInfo(outputPath).Length.Should().BeGreaterThan(0);
    }

    /// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
    [Fact]
    public async Task CoReports_Notification_ShouldCreatePdfFile()
    {
        var outputPath = Path.Combine(_outputDir, "notification.pdf");
        var definition = CreateNotificationDefinition();

        var result = await _coReportsService.RenderAsync(definition, outputPath);

        result.Format.Should().Be("pdf");
        result.PageCount.Should().Be(2);
        File.Exists(outputPath).Should().BeTrue();
        new FileInfo(outputPath).Length.Should().BeGreaterThan(0);
    }

    /// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
    [Fact]
    public async Task CoReports_AccountingSlip_ShouldCreatePdfFile()
    {
        var outputPath = Path.Combine(_outputDir, "accounting_slip.pdf");
        var definition = CreateAccountingSlipDefinition();

        var result = await _coReportsService.RenderAsync(definition, outputPath);

        result.Format.Should().Be("pdf");
        result.PageCount.Should().Be(1);
        File.Exists(outputPath).Should().BeTrue();
        new FileInfo(outputPath).Length.Should().BeGreaterThan(0);
    }

    // ------------------------------------------------------------------ CSV fallback tests
    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task CsvFormService_ShouldCreateCsvWithAllFields()
    {
        var outputPath = Path.Combine(_outputDir, "cover_letter.csv");
        var definition = CreateCoverLetterDefinition();

        var result = await _csvService.RenderAsync(definition, outputPath);

        result.Format.Should().Be("csv");
        result.PageCount.Should().Be(1);
        File.Exists(outputPath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(outputPath);
        content.Should().Contain("RecipientName");
        content.Should().Contain("テスト株式会社");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task CsvFormService_PdfRequest_ShouldThrowNotSupportedException()
    {
        var outputPath = Path.Combine(_outputDir, "cover_letter.pdf");
        var definition = CreateCoverLetterDefinition();

        var act = () => _csvService.RenderAsync(definition, outputPath);

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*シーオーリポーツ帳票クリエータ*");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task CsvFormService_MultiPageDefinition_ShouldWriteAllPages()
    {
        var outputPath = Path.Combine(_outputDir, "notification_2pages.csv");
        var definition = CreateNotificationDefinition();

        var result = await _csvService.RenderAsync(definition, outputPath);

        result.PageCount.Should().Be(2);
        var content = await File.ReadAllTextAsync(outputPath);
        content.Should().Contain("# Page 1");
        content.Should().Contain("# Page 2");
    }

    // ------------------------------------------------------------------ DI test
    /// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
    [Fact]
    public void DI_AddReportingCore_ResolvesFormCsvService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddReportingCore();

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IFormReportService>();

        resolved.Should().BeOfType<CsvFormReportService>();
    }

    /// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
    [Fact]
    public void DI_AddReportingServices_ResolvesFormCoReportsService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddReportingServices();

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IFormReportService>();

        resolved.Should().BeOfType<UnifiedAccount.Reporting.Services.CoReportsFormReportService>();
    }

    // ------------------------------------------------------------------ helpers
    private static FormReportDefinition CreateCoverLetterDefinition() =>
        new FormReportDefinition(
            "TEST_COVER",
            ReportTemplates.CoverLetter,
            [
                new FormPage(new Dictionary<string, object?>
                {
                    ["RecipientName"] = "テスト株式会社 御中",
                    ["SenderName"]    = "統一口座振替システム",
                    ["Date"]          = new DateOnly(2026, 3, 18),
                    ["Subject"]       = "振替結果のご通知",
                    ["Body"]          = "平素より大変お世話になっております。振替処理結果をご報告いたします。"
                })
            ]);

    private static FormReportDefinition CreateNotificationDefinition() =>
        new FormReportDefinition(
            "TEST_NOTIF",
            ReportTemplates.Notification,
            [
                new FormPage(new Dictionary<string, object?>
                {
                    ["RecipientName"] = "A組合 御中",
                    ["Title"]         = "口座振替処理完了のお知らせ",
                    ["Body"]          = "今月分の振替処理が正常に完了しました。",
                    ["Note"]          = "不能件数: 2件"
                }),
                new FormPage(new Dictionary<string, object?>
                {
                    ["RecipientName"] = "B組合 御中",
                    ["Title"]         = "口座振替処理完了のお知らせ",
                    ["Body"]          = "今月分の振替処理が正常に完了しました。",
                    ["Note"]          = "不能件数: 0件"
                })
            ]);

    private static FormReportDefinition CreateAccountingSlipDefinition() =>
        new FormReportDefinition(
            "TEST_SLIP",
            ReportTemplates.AccountingSlip,
            [
                new FormPage(new Dictionary<string, object?>
                {
                    ["SlipNumber"] = "2026031800001",
                    ["Date"]       = new DateOnly(2026, 3, 18),
                    ["Entries"]    = "普通預金\t1,000,000\t振替収入\t1,000,000\t3月振替",
                    ["Total"]      = "1,000,000"
                })
            ]);
}










