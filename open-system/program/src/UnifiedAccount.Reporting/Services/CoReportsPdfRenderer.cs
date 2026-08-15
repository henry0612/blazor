using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnifiedAccount.Reporting.Models;

namespace UnifiedAccount.Reporting.Services;

internal static partial class CoReportsPdfRenderer
{
    private const string PdfHeader = "%PDF-1.4\n";

    internal static void RenderPdf(string outputPath, string title, IReadOnlyList<string> bodyLines)
    {
        var objects = BuildObjects(title, bodyLines);
        var offsets = new List<int> { 0 };

        using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true);

        writer.Write(PdfHeader);
        foreach (var obj in objects)
        {
            writer.Flush();
            offsets.Add((int)stream.Position);
            writer.Write(obj);
            writer.Flush();
        }

        var xrefStart = (int)stream.Position;
        writer.WriteLine("xref");
        writer.WriteLine($"0 {offsets.Count}");
        writer.WriteLine("0000000000 65535 f ");
        foreach (var offset in offsets.Skip(1))
            writer.WriteLine($"{offset:0000000000} 00000 n ");

        writer.WriteLine("trailer");
        writer.WriteLine($"<< /Size {offsets.Count} /Root 1 0 R >>");
        writer.WriteLine("startxref");
        writer.WriteLine(xrefStart);
        writer.WriteLine("%%EOF");
        writer.Flush();
    }

    internal static IReadOnlyList<string> BuildTabularBody(TabularReportDefinition definition)
    {
        var lines = new List<string> { definition.Title };
        lines.Add(string.Join(" | ", definition.Columns.Select(c => c.Header)));

        foreach (var row in definition.Rows)
        {
            lines.Add(string.Join(" | ", definition.Columns.Select(c =>
                ToDisplayValue(row.TryGetValue(c.Key, out var value) ? value : null))));
        }

        if (definition.SummaryRows is { Count: > 0 })
        {
            lines.Add(string.Empty);
            lines.Add("Summary");
            foreach (var row in definition.SummaryRows)
                lines.Add(string.Join(" | ", definition.Columns.Select(c =>
                    ToDisplayValue(row.TryGetValue(c.Key, out var value) ? value : null))));
        }

        return lines;
    }

    internal static IReadOnlyList<string> BuildFormBody(FormReportDefinition definition)
    {
        var lines = new List<string> { definition.TemplateName };

        foreach (var page in definition.Pages)
        {
            lines.Add(string.Empty);
            foreach (var (key, value) in page.Fields)
                lines.Add($"{key}: {ToDisplayValue(value)}");
        }

        return lines;
    }

    private static string[] BuildObjects(string title, IReadOnlyList<string> bodyLines)
    {
        var pageText = string.Join("\\n", bodyLines.Select(EscapePdfText));
        var content = $"""
            BT
            /F1 12 Tf
            72 760 Td
            ({EscapePdfText(title)}) Tj
            0 -18 Td
            /F1 9 Tf
            ({pageText}) Tj
            ET
            """;

        return
        [
            "1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj\n",
            "2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj\n",
            "3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >> endobj\n",
            $"4 0 obj << /Length {Encoding.UTF8.GetByteCount(content)} >> stream\n{content}\nendstream endobj\n",
            "5 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj\n"
        ];
    }

    private static string EscapePdfText(string value)
    {
        return Regex.Replace(value, @"[\\()]", m => $"\\{m.Value}");
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
}
