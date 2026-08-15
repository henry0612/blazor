using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Reporting.Models;

namespace UnifiedAccount.Reporting.Services;

/// <summary>
/// シーオーリポーツ帳票クリエータを使った帳票レンダリング実装。
/// PDF は CoReports、CSV は軽量フォールバックで出力する。
/// </summary>
public sealed class CoReportsTabularReportService : ITabularReportService
{
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<CoReportsTabularReportService> _logger;

    public CoReportsTabularReportService(ILogger<CoReportsTabularReportService> logger)
    {
        _logger = logger;
    }

    public async Task<ReportRenderResult> RenderAsync(
        TabularReportDefinition definition,
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
            "[{ReportId}] CoReports出力開始 Format={Format} Path={Path} Rows={RowCount}",
            definition.ReportId,
            format,
            outputPath,
            definition.Rows.Count);

        if (format == "pdf")
        {
            await Task.Run(
                () => CoReportsPdfRenderer.RenderPdf(
                    outputPath,
                    definition.Title,
                    CoReportsPdfRenderer.BuildTabularBody(definition)),
                ct);
        }
        else
        {
            await RenderCsvAsync(definition, outputPath, ct);
        }

        return new ReportRenderResult(outputPath, format, definition.Rows.Count);
    }

    private static async Task RenderCsvAsync(
        TabularReportDefinition definition,
        string outputPath,
        CancellationToken ct)
    {
        await using var writer = new StreamWriter(outputPath, false, new UTF8Encoding(false));
        await writer.WriteLineAsync(string.Join(",", definition.Columns.Select(c => EscapeCsv(c.Header))));

        foreach (var row in definition.Rows)
        {
            ct.ThrowIfCancellationRequested();
            var line = string.Join(",", definition.Columns.Select(c =>
                EscapeCsv(ToDisplayValue(row.TryGetValue(c.Key, out var val) ? val : null))));
            await writer.WriteLineAsync(line);
        }

        if (definition.SummaryRows is { Count: > 0 })
        {
            foreach (var row in definition.SummaryRows)
            {
                ct.ThrowIfCancellationRequested();
                var line = string.Join(",", definition.Columns.Select(c =>
                    EscapeCsv(ToDisplayValue(row.TryGetValue(c.Key, out var val) ? val : null))));
                await writer.WriteLineAsync(line);
            }
        }
    }

    private static string ToDisplayValue(object? value) => value switch
    {
        null => string.Empty,
        DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        decimal number => number.ToString(CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static string EscapeCsv(string value)
    {
        if (!(value.Contains(',') || value.Contains('"') || value.Contains('\r') || value.Contains('\n')))
            return value;
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
