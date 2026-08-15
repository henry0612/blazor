using System.Text;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Reporting.Models;

namespace UnifiedAccount.Reporting.Services;

/// <summary>
/// シーオーリポーツ帳票クリエータを使った固定レイアウト帳票実装。
/// PDF は CoReports、CSV は軽量フォールバックで出力する。
/// </summary>
public sealed class CoReportsFormReportService : IFormReportService
{
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<CoReportsFormReportService> _logger;

    public CoReportsFormReportService(ILogger<CoReportsFormReportService> logger)
    {
        _logger = logger;
    }

    public async Task<FormRenderResult> RenderAsync(
        FormReportDefinition definition,
        string outputPath,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(definition);

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var extension = Path.GetExtension(outputPath);
        var format = string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase)
            ? "pdf"
            : "csv";

        _logger.LogInformation(
            "[{ReportId}] CoReports固定帳票出力開始 Template={Template} Format={Format} Pages={PageCount}",
            definition.ReportId,
            definition.TemplateName,
            format,
            definition.Pages.Count);

        if (format == "pdf")
        {
            await Task.Run(
                () => CoReportsPdfRenderer.RenderPdf(
                    outputPath,
                    definition.TemplateName,
                    CoReportsPdfRenderer.BuildFormBody(definition)),
                ct);
        }
        else
        {
            await RenderCsvAsync(definition, outputPath, ct);
        }

        return new FormRenderResult(outputPath, format, definition.Pages.Count);
    }

    private static async Task RenderCsvAsync(
        FormReportDefinition definition,
        string outputPath,
        CancellationToken ct)
    {
        await using var writer = new StreamWriter(outputPath, false, new UTF8Encoding(false));

        for (var pageIndex = 0; pageIndex < definition.Pages.Count; pageIndex++)
        {
            ct.ThrowIfCancellationRequested();
            var page = definition.Pages[pageIndex];

            if (pageIndex > 0)
                await writer.WriteLineAsync();

            await writer.WriteLineAsync($"# Page {pageIndex + 1}");
            await writer.WriteLineAsync($"ReportId,{EscapeCsv(definition.ReportId)}");
            await writer.WriteLineAsync($"Template,{EscapeCsv(definition.TemplateName)}");

            foreach (var (key, value) in page.Fields)
            {
                ct.ThrowIfCancellationRequested();
                await writer.WriteLineAsync($"{EscapeCsv(key)},{EscapeCsv(ToDisplayValue(value))}");
            }
        }
    }

    private static string ToDisplayValue(object? value) => value switch
    {
        null => string.Empty,
        DateOnly date => date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
        DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
        decimal number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static string EscapeCsv(string value)
    {
        if (!(value.Contains(',') || value.Contains('"') || value.Contains('\r') || value.Contains('\n')))
            return value;
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
