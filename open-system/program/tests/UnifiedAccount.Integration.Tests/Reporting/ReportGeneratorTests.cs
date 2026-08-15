using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using UnifiedAccount.Application.Services.Reports;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Infrastructure.Services.Reports;

namespace UnifiedAccount.Integration.Tests.Reporting;

public class ReportGeneratorTests
{
    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task GenerateReportAsync_PdfOutput_ShouldCreatePdfFile()
    {
        var sampleDir = LocateSampleDirectory();
        var workDir = Path.Combine(Path.GetTempPath(), $"correports_gen_{Guid.NewGuid():N}");
        Directory.CreateDirectory(workDir);

        try
        {
            foreach (var sourceFile in Directory.GetFiles(sampleDir, "sample.*"))
            {
                var destination = Path.Combine(workDir, Path.GetFileName(sourceFile));
                File.Copy(sourceFile, destination, true);
            }

            var generator = new ReportGenerator(NullLogger<ReportGenerator>.Instance);
            var outputPath = Path.Combine(workDir, "result.pdf");

            var template = new ReportTemplate
            {
                TemplateId = "TEST-PDF",
                TemplateName = "Sample PDF",
                TemplateFilePath = Path.Combine(workDir, "sample.dcx"),
                OutputFormat = "PDF",
                IsActive = true,
                DetailFields = new[] { "Text1", "Text2" }
            };

            var request = new GenerateReportRequest
            {
                ExecutionId = $"EXEC-{Guid.NewGuid():N}",
                ReportDataId = "REPORT-001",
                Template = template,
                DataSource = new ReportDataSet
                {
                    TemplateId = template.TemplateId,
                    HeaderData = new Dictionary<string, object>
                    {
                        ["Text2"] = "データ"
                    },
                    DetailRows = new List<Dictionary<string, object>>
                    {
                        new()
                        {
                            ["Text1"] = "0",
                            ["Text2"] = "データ"
                        }
                    },
                    SummaryData = new Dictionary<string, object>()
                },
                Mode = ReportDataMode.Buffered,
                OutputFilePath = outputPath,
                OutputFormat = "PDF"
            };

            var result = await generator.GenerateReportAsync(request);

            result.GenerationException.Should().BeNull();
            result.IsPartialOutput.Should().BeFalse();
            result.OutputFilePath.Should().Be(outputPath);
            result.FileSizeBytes.Should().BeGreaterThan(0);
            File.Exists(outputPath).Should().BeTrue();
            (await File.ReadAllBytesAsync(outputPath)).Take(4).Should().Equal((byte)'%', (byte)'P', (byte)'D', (byte)'F');

            var loaded = await generator.GetGeneratedReportAsync(request.ExecutionId);
            loaded.ExecutionId.Should().Be(request.ExecutionId);
            loaded.OutputFilePath.Should().Be(outputPath);
            loaded.FileSizeBytes.Should().BeGreaterThan(0);
        }
        finally
        {
            if (Directory.Exists(workDir))
                Directory.Delete(workDir, true);
        }
    }

    private static string LocateSampleDirectory()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            var candidate = Path.Combine(current.FullName, "open-system", "program", "tools", "CoReportsSample");
            if (Directory.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("CoReportsSample ディレクトリを見つけられない。");
    }
}










