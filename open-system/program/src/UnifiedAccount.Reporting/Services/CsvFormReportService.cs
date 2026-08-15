using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Reporting.Models;

namespace UnifiedAccount.Reporting.Services;

/// <summary>
/// エンジン不要の CSV のみ出力実装 (フォールバック)。
/// フィールドをキー,値形式で1ページごとに出力する。
/// PDF 出力要求時は <see cref="NotSupportedException"/> をスロー。
/// </summary>
public sealed class CsvFormReportService : IFormReportService
{
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<CsvFormReportService> _logger;

    public CsvFormReportService(ILogger<CsvFormReportService> logger)
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

        var extension = Path.GetExtension(outputPath);
        if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException(
                "PDF出力にはシーオーリポーツ帳票クリエータ プロバイダの登録が必要です。");

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        _logger.LogInformation(
            "[{ReportId}] 固定帳票CSV出力開始 Template={Template} Path={Path} Pages={PageCount}",
            definition.ReportId, definition.TemplateName, outputPath, definition.Pages.Count);

        await using var writer = new StreamWriter(outputPath, false, new UTF8Encoding(false));

        for (var pageIndex = 0; pageIndex < definition.Pages.Count; pageIndex++)
        {
            ct.ThrowIfCancellationRequested();
            var page = definition.Pages[pageIndex];

            if (pageIndex > 0)
                await writer.WriteLineAsync(); // ページ区切り

            await writer.WriteLineAsync($"# Page {pageIndex + 1}");
            await writer.WriteLineAsync($"ReportId,{EscapeCsv(definition.ReportId)}");
            await writer.WriteLineAsync($"Template,{EscapeCsv(definition.TemplateName)}");

            foreach (var (key, value) in page.Fields)
            {
                ct.ThrowIfCancellationRequested();
                await writer.WriteLineAsync($"{EscapeCsv(key)},{EscapeCsv(ToDisplayValue(value))}");
            }
        }

        return new FormRenderResult(outputPath, "csv", definition.Pages.Count);
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
