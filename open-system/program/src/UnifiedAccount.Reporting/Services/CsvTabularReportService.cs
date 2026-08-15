using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Reporting.Models;

namespace UnifiedAccount.Reporting.Services;

/// <summary>
/// エンジン不要の CSV のみ出力実装 (フォールバック)。
/// PDF 出力要求時は NotSupportedException をスロー。
/// PDF 対応エンジンを使う場合は上位のプロバイダで上書き登録する。
/// </summary>
public sealed class CsvTabularReportService : ITabularReportService
{
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<CsvTabularReportService> _logger;

    public CsvTabularReportService(ILogger<CsvTabularReportService> logger)
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

        var extension = Path.GetExtension(outputPath);
        if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException(
                "PDF出力にはシーオーリポーツ帳票クリエータ プロバイダの登録が必要です。");

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        _logger.LogInformation(
            "[{ReportId}] CSV出力開始 Path={Path} Rows={RowCount}",
            definition.ReportId, outputPath, definition.Rows.Count);

        await RenderCsvAsync(definition, outputPath, ct);

        return new ReportRenderResult(outputPath, "csv", definition.Rows.Count);
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

    internal static string ToDisplayValue(object? value) => value switch
    {
        null => string.Empty,
        DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        decimal number => number.ToString(CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    internal static string EscapeCsv(string value)
    {
        if (!(value.Contains(',') || value.Contains('"') || value.Contains('\r') || value.Contains('\n')))
            return value;
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
