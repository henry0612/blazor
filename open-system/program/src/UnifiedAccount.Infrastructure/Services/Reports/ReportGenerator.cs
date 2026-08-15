using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using System.Collections.Concurrent;
using Hos.DocCreator;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Application.Services.Reports;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Domain.Interfaces.Reports;

namespace UnifiedAccount.Infrastructure.Services.Reports;

/// <summary>
/// CoReports 帳票クリエータを使用して、PDF/PRN 形式の帳票を生成する。
/// Buffered/Streamed 両モードを実呼び出し化する。
/// </summary>
public class ReportGenerator : IReportGenerator
{
    private static readonly ConcurrentDictionary<string, GeneratedReport> GeneratedReports = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<ReportGenerator> _logger;

    public ReportGenerator(ILogger<ReportGenerator> logger)
    {
        _logger = logger;
    }

    public async Task<GeneratedReport> GenerateReportAsync(
        GenerateReportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Template);
        ArgumentNullException.ThrowIfNull(request.DataSource);

        if (string.IsNullOrWhiteSpace(request.OutputFilePath))
            throw new ArgumentException("OutputFilePath は必須です。", nameof(request));

        var report = new GeneratedReport
        {
            ExecutionId = request.ExecutionId,
            ReportDataId = request.ReportDataId,
            TemplateId = request.Template.TemplateId,
            OutputFormat = ResolveOutputFormat(request.OutputFilePath, request.OutputFormat)
        };

        var outputPath = Path.GetFullPath(request.OutputFilePath);
        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        var workDirectory = Path.Combine(
            Path.GetTempPath(),
            "UnifiedAccount",
            "CoReports",
            request.ExecutionId ?? Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDirectory);

        string? runtimeConfigPath = null;
        string? tempDataPath = null;
        CnDocCreator? creator = null;

        try
        {
            _logger.LogInformation(
                "[{ReportId}] CoReports出力開始 Format={Format} Path={Path}",
                request.Template.TemplateId,
                report.OutputFormat,
                outputPath);

            var prepared = await PrepareInputsAsync(
                request,
                workDirectory,
                outputPath,
                cancellationToken);

            runtimeConfigPath = prepared.RuntimeConfigPath;
            tempDataPath = prepared.TempDataPath;

            EnsureCoReportsNativeRuntime();
            creator = new CnDocCreator();
            creator.LoadConfiguration(runtimeConfigPath);
            creator.Configuration.UserDefined = prepared.UserDefined;
            creator.Initialize();

            SetOutputNames(creator, outputPath);

            await Task.Run(() => creator.Output(), cancellationToken);

            var producedFilePath = ResolveProducedFilePath(creator, outputPath);
            if (!File.Exists(producedFilePath))
                throw new InvalidOperationException($"CoReports 出力結果が見つからない: {producedFilePath}");

            if (!Path.GetFullPath(producedFilePath).Equals(outputPath, StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(producedFilePath, outputPath, true);
                producedFilePath = outputPath;
            }

            report.OutputFilePath = producedFilePath;
            report.FileSizeBytes = new FileInfo(producedFilePath).Length;
            RememberGeneratedReport(report);
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CoReports 帳票生成エラー");
            report.GenerationException = ex;

            if (TryResolveProducedFilePath(creator, outputPath, out var producedFilePath)
                && File.Exists(producedFilePath))
            {
                report.OutputFilePath = producedFilePath;
                report.IsPartialOutput = true;
                report.FileSizeBytes = new FileInfo(producedFilePath).Length;
            }
            else
            {
                report.FileSizeBytes = 0;
            }

            RememberGeneratedReport(report);
            return report;
        }
        finally
        {
            DeleteIfExists(runtimeConfigPath);
            DeleteIfExists(tempDataPath);
        }
    }

    public async Task<GeneratedReport> GetGeneratedReportAsync(
        string executionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(executionId))
            throw new ArgumentException("executionId は必須です。", nameof(executionId));

        cancellationToken.ThrowIfCancellationRequested();

        if (!GeneratedReports.TryGetValue(executionId, out var report))
            throw new KeyNotFoundException($"ExecutionId '{executionId}' の帳票結果が存在しない。");

        return await Task.FromResult(Clone(report));
    }

    private static void EnsureCoReportsNativeRuntime()
    {
        var runtimeLibraryNames = new[]
        {
            "vcruntime140_cor3.dll",
            "vcruntime140.dll"
        };

        if (runtimeLibraryNames.Any(runtimeLibraryName => TryLoadNativeLibrary(runtimeLibraryName)))
            return;

        var searchRoots = new[]
        {
            AppContext.BaseDirectory,
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PowerShell", "7"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "PowerShell", "7"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Visual Studio", "2022"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "Visual Studio", "2022")
        };

        foreach (var searchRoot in searchRoots.Where(Directory.Exists))
        {
            foreach (var runtimeLibraryName in runtimeLibraryNames)
            {
                var candidatePath = Path.Combine(searchRoot, runtimeLibraryName);
                if (TryLoadNativeLibrary(candidatePath))
                    return;
            }
        }

        throw new InvalidOperationException(
            "CoReports のネイティブ実行に必要な VC++ ランタイムを読み込めませんでした。" +
            "Visual C++ Redistributable for Visual Studio 2015-2022 (x64) をインストールするか、" +
            "vcruntime140_cor3.dll を実行パスに追加してください。");
    }

    private static bool TryLoadNativeLibrary(string libraryPathOrName)
    {
        try
        {
            return NativeLibrary.TryLoad(libraryPathOrName, out _);
        }
        catch (DllNotFoundException)
        {
            return false;
        }
        catch (BadImageFormatException)
        {
            return false;
        }
    }

    private async Task<PreparedInputs> PrepareInputsAsync(
        GenerateReportRequest request,
        string workDirectory,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var template = request.Template;
        var dataSet = request.DataSource as ReportDataSet;
        var recordSet = request.DataSource as CnRecordSet;

        var csvPath = Path.Combine(workDirectory, $"{Path.GetFileNameWithoutExtension(outputPath)}.csv");
        var fieldOrder = ResolveFieldOrder(template, dataSet, request);
        var userDefined = BuildUserDefinedValues(request, dataSet);

        if (dataSet != null)
        {
            await WriteBufferedCsvAsync(csvPath, fieldOrder, dataSet, cancellationToken);
        }
        else if (recordSet != null)
        {
            using (recordSet)
            {
                await WriteStreamedCsvAsync(csvPath, fieldOrder, recordSet, cancellationToken);
            }
        }
        else
        {
            throw new InvalidOperationException(
                $"未対応 DataSource: {request.DataSource.GetType().FullName}");
        }

        var runtimeConfigPath = PrepareRuntimeConfiguration(
            template.TemplateFilePath,
            csvPath,
            outputPath,
            request.OutputFormat);

        return new PreparedInputs(runtimeConfigPath, csvPath, userDefined);
    }

    private static async Task WriteBufferedCsvAsync(
        string csvPath,
        IReadOnlyList<string> fieldOrder,
        ReportDataSet dataSet,
        CancellationToken cancellationToken)
    {
        var rows = dataSet.DetailRows.Count > 0
            ? dataSet.DetailRows.Select(row => row.ToDictionary(k => k.Key, v => (object?)v.Value, StringComparer.OrdinalIgnoreCase))
            : new[]
            {
                MergeRow(
                    dataSet.HeaderData,
                    dataSet.SummaryData)
            };

        await WriteCsvAsync(csvPath, fieldOrder, rows, cancellationToken);
    }

    private static async Task WriteStreamedCsvAsync(
        string csvPath,
        IReadOnlyList<string> fieldOrder,
        CnRecordSet recordSet,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(csvPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(false));

        await writer.WriteLineAsync(string.Join(",", fieldOrder.Select(EscapeCsv)));

        while (!recordSet.Eof)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in fieldOrder)
                row[field] = recordSet.GetValue(field);

            await writer.WriteLineAsync(string.Join(",", fieldOrder.Select(field =>
                EscapeCsv(ToDisplayValue(row.TryGetValue(field, out var value) ? value : null)))));

            if (!recordSet.Next())
                break;
        }
    }

    private static async Task WriteCsvAsync(
        string csvPath,
        IReadOnlyList<string> fieldOrder,
        IEnumerable<IReadOnlyDictionary<string, object?>> rows,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(csvPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(false));

        await writer.WriteLineAsync(string.Join(",", fieldOrder.Select(EscapeCsv)));

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await writer.WriteLineAsync(string.Join(",", fieldOrder.Select(field =>
                EscapeCsv(ToDisplayValue(row.TryGetValue(field, out var value) ? value : null)))));
        }
    }

    private static IReadOnlyList<string> ResolveFieldOrder(
        ReportTemplate template,
        ReportDataSet? dataSet,
        GenerateReportRequest request)
    {
        var fields = new List<string>();
        AddDistinct(fields, template.RequiredFields);
        AddDistinct(fields, template.DetailFields);
        AddDistinct(fields, template.SummaryFields);

        if (fields.Count > 0)
            return fields;

        if (dataSet?.DetailRows.Count > 0)
            return dataSet.DetailRows[0].Keys.ToList();

        if (dataSet?.HeaderData.Count > 0)
            return dataSet.HeaderData.Keys.ToList();

        if (request.HeaderData is { Count: > 0 })
            return request.HeaderData.Keys.ToList();

        if (request.SummaryData is { Count: > 0 })
            return request.SummaryData.Keys.ToList();

        var templateFields = ResolveFieldOrderFromTemplate(template.TemplateFilePath);
        if (templateFields.Count > 0)
            return templateFields;

        throw new InvalidOperationException("帳票フィールド定義が不足している。");
    }

    private static IReadOnlyList<string> ResolveFieldOrderFromTemplate(string? templateFilePath)
    {
        if (string.IsNullOrWhiteSpace(templateFilePath) || !File.Exists(templateFilePath))
            return Array.Empty<string>();

        var document = XDocument.Load(templateFilePath);
        var fields = new List<string>();

        foreach (var fieldOrder in document
                     .Descendants()
                     .Where(e =>
                         string.Equals(e.Name.LocalName, "Property", StringComparison.OrdinalIgnoreCase) &&
                         string.Equals((string?)e.Attribute("name"), "FieldOrder", StringComparison.OrdinalIgnoreCase))
                     .SelectMany(e => e.Descendants())
                     .Where(e => string.Equals(e.Name.LocalName, "Property", StringComparison.OrdinalIgnoreCase))
                     .Select(e => (string?)e.Attribute("value"))
                     .Where(v => !string.IsNullOrWhiteSpace(v)))
        {
            foreach (var field in fieldOrder!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                AddDistinct(fields, new[] { field });
        }

        // データソース：文字列指定
        foreach (var fieldName in document
                     .Descendants()
                     .Where(e =>
                        string.Equals(e.Name.LocalName, "Source", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals((string?)e.Attribute("valueAs"), "DATA", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals((string?)e.Attribute("valueType"), "TEXT", StringComparison.OrdinalIgnoreCase))
                     .Select(e => (string?)e.Attribute("value"))
                     .Where(v => !string.IsNullOrWhiteSpace(v)))
        {
            foreach (var field in fieldName!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                AddDistinct(fields, new[] { field });
        }

        // フィールド名
        foreach (var fieldName in document
                     .Descendants()
                     .Where(e => string.Equals(e.Name.LocalName, "Field", StringComparison.OrdinalIgnoreCase))
                     .Select(e => (string?)e.Attribute("name"))
                     .Where(v => !string.IsNullOrWhiteSpace(v)))
        {
            AddDistinct(fields, new[] { fieldName! });
        }

        // フィールドデータソース
        foreach (var fieldName in document
                     .Descendants()
                     .Where(e =>
                        string.Equals(e.Name.LocalName, "Field", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals((string?)e.Attribute("valueAs"), "DATA", StringComparison.OrdinalIgnoreCase))
                     .Select(e => (string?)e.Value)
                     .Where(v => !string.IsNullOrWhiteSpace(v)))
        {
            AddDistinct(fields, new[] { fieldName! });
        }

        return fields;
    }

    private static void AddDistinct(List<string> target, IEnumerable<string> source)
    {
        foreach (var item in source)
        {
            if (!target.Any(existing => string.Equals(existing, item, StringComparison.OrdinalIgnoreCase)))
                target.Add(item);
        }
    }

    private static Dictionary<string, string> BuildUserDefinedValues(
        GenerateReportRequest request,
        ReportDataSet? dataSet)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        MergeUserDefined(values, request.HeaderData);
        MergeUserDefined(values, request.SummaryData);

        if (dataSet != null)
        {
            MergeUserDefined(values, dataSet.HeaderData);
            MergeUserDefined(values, dataSet.SummaryData);
        }

        return values;
    }

    private static void MergeUserDefined(
        IDictionary<string, string> target,
        IDictionary<string, object>? source)
    {
        if (source == null)
            return;

        foreach (var (key, value) in source)
            target[key] = ToDisplayValue(value);
    }

    private static Dictionary<string, object?> MergeRow(
        IDictionary<string, object> headerData,
        IDictionary<string, object> summaryData)
    {
        var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in headerData)
            row[key] = value;
        foreach (var (key, value) in summaryData)
            row[key] = value;
        return row;
    }

    private static string PrepareRuntimeConfiguration(
        string templateFilePath,
        string csvPath,
        string outputPath,
        string? requestedOutputFormat)
    {
        if (!File.Exists(templateFilePath))
            throw new FileNotFoundException("テンプレートが見つからない。", templateFilePath);

        var templateDirectory = Path.GetDirectoryName(Path.GetFullPath(templateFilePath))
            ?? throw new InvalidOperationException("テンプレート配置先を解決できない。");
        var runtimeConfigPath = Path.Combine(
            templateDirectory,
            $"{Path.GetFileNameWithoutExtension(templateFilePath)}.runtime.{Guid.NewGuid():N}{Path.GetExtension(templateFilePath)}");

        var document = XDocument.Load(templateFilePath, LoadOptions.PreserveWhitespace);
        var root = document.Root ?? throw new InvalidOperationException("テンプレート XML が空。");
        var ns = root.Name.Namespace;
        var fileType = ResolveFileType(requestedOutputFormat, outputPath);

        foreach (var fileOutJob in document.Descendants(ns + "FileOutJob"))
        {
            fileOutJob.SetAttributeValue("fileName", Path.GetFullPath(Path.ChangeExtension(outputPath, null)));

            var documentElement = fileOutJob.Element(ns + "Document");
            if (documentElement != null)
                documentElement.SetAttributeValue("type", fileType);
        }

        foreach (var recordSet in document.Descendants(ns + "RecordSet"))
        {
            var type = ((string?)recordSet.Attribute("type")) ?? string.Empty;
            if (!string.Equals(type, "CSV", StringComparison.OrdinalIgnoreCase))
                continue;

            var source = recordSet.Element(ns + "Source");
            if (source == null)
                continue;

            source.SetAttributeValue("valueAs", "FILE");
            source.SetAttributeValue("value", csvPath);
        }

        document.Save(runtimeConfigPath);
        return runtimeConfigPath;
    }

    private static string ResolveOutputFormat(string outputPath, string? outputFormat)
    {
        var extension = Path.GetExtension(outputPath);
        if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            return "pdf";

        if (string.Equals(outputFormat, "PDF", StringComparison.OrdinalIgnoreCase))
            return "pdf";

        return "prn";
    }

    private static string ResolveFileType(string? requestedOutputFormat, string outputPath)
    {
        if (string.Equals(Path.GetExtension(outputPath), ".pdf", StringComparison.OrdinalIgnoreCase))
            return "PDF";

        if (string.Equals(requestedOutputFormat, "PDF", StringComparison.OrdinalIgnoreCase))
            return "PDF";

        return "BINARY";
    }

    private static void SetOutputNames(CnDocCreator creator, string outputPath)
    {
        var outputName = Path.GetFileNameWithoutExtension(outputPath);
        foreach (var job in creator.CnJobs)
            job.Value.OutputName = outputName;
    }

    private static string ResolveProducedFilePath(CnDocCreator creator, string outputPath)
    {
        if (TryResolveProducedFilePath(creator, outputPath, out var producedFilePath))
            return producedFilePath;

        throw new InvalidOperationException("CoReports 出力ファイルを特定できない。");
    }

    private static bool TryResolveProducedFilePath(
        CnDocCreator? creator,
        string outputPath,
        out string producedFilePath)
    {
        producedFilePath = outputPath;

        if (creator == null || creator.DocumentFileNames == null)
            return File.Exists(producedFilePath);

        foreach (var candidate in creator.DocumentFileNames.Values)
        {
            if (File.Exists(candidate))
            {
                producedFilePath = candidate;
                return true;
            }
        }

        return File.Exists(producedFilePath);
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

    private void DeleteIfExists(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "一時ファイル削除失敗: {Path}", path);
        }
    }

    private static void RememberGeneratedReport(GeneratedReport report)
    {
        if (string.IsNullOrWhiteSpace(report.ExecutionId))
            return;

        GeneratedReports[report.ExecutionId] = Clone(report);
    }

    private static GeneratedReport Clone(GeneratedReport source)
    {
        return new GeneratedReport
        {
            ExecutionId = source.ExecutionId,
            ReportDataId = source.ReportDataId,
            TemplateId = source.TemplateId,
            OutputFormat = source.OutputFormat,
            OutputFilePath = source.OutputFilePath,
            FileSizeBytes = source.FileSizeBytes,
            IsPartialOutput = source.IsPartialOutput,
            GenerationException = source.GenerationException
        };
    }

    private sealed record PreparedInputs(
        string RuntimeConfigPath,
        string TempDataPath,
        Dictionary<string, string> UserDefined);
}
