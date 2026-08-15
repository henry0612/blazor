using System.Collections;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Application.Services;
using UnifiedAccount.Domain.Constants;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Application.Services.Reports;
using UnifiedAccount.Domain.Interfaces.Reports;

namespace UnifiedAccount.Infrastructure.Services.Reports;

/// <summary>
/// 帳票出力のメインオーケストレータ。
/// ステップ1～7: テンプレート検証 → データ組立 → 生成 → 出力 → 結果記録 → リトライまで統合。
/// COBOL移行元: SYSMAIN (帳票出力主処理)
/// </summary>
public class ReportOutputService : IReportOutputService
{
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<ReportOutputService> _logger;
    /// <summary>
    /// テンプレート解決サービスを保持する。
    /// </summary>
    private readonly IReportTemplateResolver _templateResolver;
    /// <summary>
    /// データ生成サービスを保持する。
    /// </summary>
    private readonly IReportDataBuilder _dataBuilder;
    /// <summary>
    /// 生成サービスを保持する。
    /// </summary>
    private readonly IReportGenerator _generator;
    /// <summary>
    /// 出力出力先を保持する。
    /// </summary>
    private readonly IReportOutputDestination _outputDestination;
    /// <summary>
    /// 実行サービスを保持する。
    /// </summary>
    private readonly IReportExecutionResultService _executionService;
    /// <summary>
    /// 実行番号サービスを保持する。
    /// </summary>
    private readonly IExecutionNumberService _executionNumberService;
    /// <summary>
    /// 出力ファイルパス生成サービスを保持する。
    /// </summary>
    private readonly IReportFilePathProvider? _filePathProvider;

    public ReportOutputService(
        ILogger<ReportOutputService> logger,
        IReportTemplateResolver templateResolver,
        IReportDataBuilder dataBuilder,
        IReportGenerator generator,
        IReportOutputDestination outputDestination,
        IReportExecutionResultService executionService)
        : this(logger, templateResolver, dataBuilder, generator, outputDestination, executionService, new ExecutionNumberService(), null)
    {
    }

    public ReportOutputService(
        ILogger<ReportOutputService> logger,
        IReportTemplateResolver templateResolver,
        IReportDataBuilder dataBuilder,
        IReportGenerator generator,
        IReportOutputDestination outputDestination,
        IReportExecutionResultService executionService,
        IReportFilePathProvider filePathProvider)
        : this(logger, templateResolver, dataBuilder, generator, outputDestination, executionService, new ExecutionNumberService(), filePathProvider)
    {
    }

    public ReportOutputService(
        ILogger<ReportOutputService> logger,
        IReportTemplateResolver templateResolver,
        IReportDataBuilder dataBuilder,
        IReportGenerator generator,
        IReportOutputDestination outputDestination,
        IReportExecutionResultService executionService,
        IExecutionNumberService executionNumberService)
        : this(logger, templateResolver, dataBuilder, generator, outputDestination, executionService, executionNumberService, null)
    {
    }

    public ReportOutputService(
        ILogger<ReportOutputService> logger,
        IReportTemplateResolver templateResolver,
        IReportDataBuilder dataBuilder,
        IReportGenerator generator,
        IReportOutputDestination outputDestination,
        IReportExecutionResultService executionService,
        IExecutionNumberService executionNumberService,
        IReportFilePathProvider? filePathProvider)
    {
        _logger = logger;
        _templateResolver = templateResolver;
        _dataBuilder = dataBuilder;
        _generator = generator;
        _outputDestination = outputDestination;
        _executionService = executionService;
        _executionNumberService = executionNumberService ?? throw new ArgumentNullException(nameof(executionNumberService));
        _filePathProvider = filePathProvider;
    }

    /// <summary>
    /// 帳票出力リクエストを受け付け、テンプレート検証、データ組立、PDF/PRN生成、出力を行う。
    /// 
    /// 処理フロー (ステップ1～7):
    /// 1. リクエスト検証
    /// 2. テンプレート検証 (IReportTemplateResolver)
    /// 3. データ組立 (IReportDataBuilder)
    /// 4. 帳票生成 (IReportGenerator)
    /// 5. 出力先ルーティング (IReportOutputDestination)
    /// 6. 実行結果記録 (IReportExecutionResultService.CompleteExecutionAsync)
    /// 7. リトライ判定 (IReportExecutionResultService.CanRetryAsync)
    /// </summary>
    /// <param name="request">リクエストを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task<ReportOutputResponse> OutputAsync(
        ReportOutputRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new ReportOutputResponse
        {
            ExecutionId = request?.ExecutionId,
            FailureKind = ReportOutputFailureKind.None
        };

        ReportExecution? execution = null;
        var currentFailureKind = ReportOutputFailureKind.Unexpected;

        try
        {
            currentFailureKind = ReportOutputFailureKind.Validation;
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            NormalizeRequest(request);
            ValidateRequest(request);

            var reportDataId = string.IsNullOrWhiteSpace(request.ReportDataId)
                ? _executionNumberService.GenerateReportDataId(request.TemplateId, request.ProcessDate)
                : request.ReportDataId;

            request.ReportDataId = reportDataId;
            response.ExecutionId = request.ExecutionId;
            response.ReportDataId = reportDataId;

            _logger.LogInformation("帳票出力開始 (ExecutionId={ExecutionId}, ReportDataId={ReportDataId}, TemplateId={TemplateId})",
                request.ExecutionId, reportDataId, request.TemplateId);

            // ステップ6: 実行開始を記録（実行前に）
            currentFailureKind = ReportOutputFailureKind.Unexpected;
            execution = await _executionService.StartExecutionAsync(
                request.ExecutionId,
                request.JobExecutionId,
                request.TemplateId,
                request.DataRequest!.Mode.ToString(),
                null,
                cancellationToken);

            // ステップ2: テンプレート検証
            var template = await ValidateTemplate(request.TemplateId, cancellationToken);

            // ステップ3: データ組立
            currentFailureKind = ReportOutputFailureKind.DataBuildFailed;
            var dataSource = await BuildData(request, template, cancellationToken);
            if (dataSource is ReportDataSet dataSet)
            {
                var validation = await _dataBuilder.ValidateDataAsync(dataSet, template, cancellationToken);
                if (!validation.IsValid)
                {
                    response.IsSuccessful = false;
                    response.FailureKind = ReportOutputFailureKind.Validation;
                    // 既存呼出側が表示している総括メッセージは互換性のため維持する。
                    response.ValidationErrors.Add("帳票生成に失敗しました。");
                    response.ValidationErrors.AddRange(validation.Errors);
                    await _executionService.FailExecutionAsync(
                        request.ExecutionId,
                        new InvalidOperationException(string.Join("; ", validation.Errors)),
                        null,
                        cancellationToken);
                    return response;
                }
            }

            // ステップ4: 帳票生成
            var splitGroups = await BuildSplitGroupsAsync(request, dataSource, cancellationToken);
            string? successfulOutputPath = null;

            for (var index = 0; index < splitGroups.Count; index++)
            {
                var group = splitGroups[index];
                var outputPath = BuildOutputPathForGroup(request, group, index);
                var groupRequest = CreateGroupRequest(request, group, outputPath);
                currentFailureKind = ReportOutputFailureKind.GenerationFailed;
                var generatedReport = await GenerateReport(groupRequest, template, group.DataSource, cancellationToken);

                if (!generatedReport.IsSuccessful())
                {
                    response.IsSuccessful = false;
                    response.FailureKind = ReportOutputFailureKind.GenerationFailed;
                    response.GeneratedReport = generatedReport;
                    response.ValidationErrors.Add("帳票生成に失敗しました。");

                    _logger.LogWarning("帳票生成失敗 (ExecutionId={ExecutionId})", request.ExecutionId);

                    await _executionService.FailExecutionAsync(
                        request.ExecutionId,
                        generatedReport.GenerationException ?? new InvalidOperationException("帳票生成に失敗しました。"),
                        generatedReport.IsPartialOutput && execution is not null ? execution.IncompleteFilePath : null,
                        cancellationToken);

                    return response;
                }

                currentFailureKind = ReportOutputFailureKind.DestinationFailed;
                var outputSuccess = await RouteOutput(groupRequest, generatedReport, cancellationToken);
                if (!outputSuccess)
                {
                    response.IsSuccessful = false;
                    response.FailureKind = ReportOutputFailureKind.DestinationFailed;
                    response.GeneratedReport = generatedReport;
                    response.ValidationErrors.Add("出力先ルーティングに失敗しました。");

                    _logger.LogWarning("出力先ルーティング失敗 (ExecutionId={ExecutionId})", request.ExecutionId);

                    await _executionService.FailExecutionAsync(
                        request.ExecutionId,
                        new InvalidOperationException("出力先ルーティング失敗"),
                        null,
                        cancellationToken);

                    return response;
                }

                successfulOutputPath ??= request.OutputDestination?.OutputPath;
                response.GeneratedReport = generatedReport;
                response.ReportDataId = generatedReport.ReportDataId;
                response.OutputFilePath = generatedReport.OutputFilePath;
            }

            response.IsSuccessful = true;
            response.FailureKind = ReportOutputFailureKind.None;

            await _executionService.CompleteExecutionAsync(
                request.ExecutionId,
                successfulOutputPath ?? request.OutputDestination?.OutputPath ?? string.Empty,
                false,
                cancellationToken);

            _logger.LogInformation("帳票出力完了 (ExecutionId={ExecutionId}, FilePath={FilePath})",
                request.ExecutionId, successfulOutputPath);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "帳票出力エラー (ExecutionId={ExecutionId})", request?.ExecutionId);

            response.IsSuccessful = false;
            response.FailureKind = ex switch
            {
                ArgumentException => ReportOutputFailureKind.Validation,
                FileNotFoundException => ReportOutputFailureKind.TemplateNotFound,
                _ => currentFailureKind
            };
            response.ValidationErrors.Add(ex.Message);

            // ステップ6: エラーを記録
            if (execution != null)
            {
                await _executionService.FailExecutionAsync(
                    execution.ExecutionId,
                    ex,
                    null,
                    cancellationToken);
            }

            return response;
        }
    }

    /// <summary>
    /// 帳票実行結果を照会する。
    /// </summary>
    /// <param name="executionId">実行IDを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task<ReportExecutionResult> GetExecutionResultAsync(
        string executionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(executionId))
            throw new ArgumentException("ExecutionId は必須です。", nameof(executionId));

        _logger.LogDebug("実行結果照会: {ExecutionId}", executionId);

        var execution = await _executionService.GetExecutionAsync(executionId, cancellationToken);

        if (execution == null)
        {
            return new ReportExecutionResult
            {
                ExecutionId = executionId,
                Status = ReportExecutionStatus.Pending
            };
        }

        return new ReportExecutionResult
        {
            ExecutionId = execution.ExecutionId,
            TemplateId = execution.TemplateId,
            Status = MapExecutionStatus(execution.Status),
            ExecutedAt = execution.CreatedAt,
            RetryCount = execution.RetryCount,
            IsPartialOutput = execution.Status == ReportExecutionStatuses.PartialOutput,
            ErrorMessage = execution.ErrorMessage,
            OutputFilePath = execution.IncompleteFilePath
        };
    }

    /// <summary>
    /// 失敗した帳票出力をリトライする。
    /// ステップ7: リトライ判定・実行
    /// </summary>
    /// <param name="executionId">実行IDを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task<ReportOutputResponse> RetryAsync(
        string executionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(executionId))
            throw new ArgumentException("ExecutionId は必須です。", nameof(executionId));

        _logger.LogInformation("帳票出力リトライ開始 (ExecutionId={ExecutionId})", executionId);

        var response = new ReportOutputResponse
        {
            ExecutionId = executionId
        };

        try
        {
            // 実行履歴から前回の情報を取得
            var execution = await _executionService.GetExecutionAsync(executionId, cancellationToken);

            if (execution == null)
            {
                response.IsSuccessful = false;
                response.ValidationErrors.Add($"実行ID '{executionId}' の履歴が見つかりません。");
                return response;
            }

            // リトライ可能性を確認
            var canRetry = await _executionService.CanRetryAsync(executionId, cancellationToken);

            if (!canRetry)
            {
                response.IsSuccessful = false;
                response.ValidationErrors.Add("リトライの最大回数に達しました。");
                _logger.LogWarning("リトライ不可 (ExecutionId={ExecutionId}, MaxRetries reached)", executionId);
                return response;
            }

            // リトライ回数を増加
            await _executionService.IncrementRetryCountAsync(executionId, cancellationToken);

            // 古い.incomplete ファイルがあればクリーンアップ（デフォルト: 7日以上前）
            await _executionService.CleanupIncompleteFilesAsync(7, cancellationToken);

            response.IsSuccessful = true;
            _logger.LogInformation("リトライ準備完了 (ExecutionId={ExecutionId}, RetryCount={RetryCount})",
                executionId, execution.RetryCount + 1);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "リトライ処理エラー (ExecutionId={ExecutionId})", executionId);
            response.IsSuccessful = false;
            response.ValidationErrors.Add(ex.Message);
            return response;
        }
    }

    private void NormalizeRequest(ReportOutputRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.OutputFormat))
            request.OutputFormat = "PDF";

        if (request.OutputDestination == null)
        {
            request.OutputDestination = new ReportOutputDestinationRequest
            {
                OutputPath = CreateDefaultOutputPath(request),
                SaveToFile = true
            };
            return;
        }

        if (string.IsNullOrWhiteSpace(request.OutputDestination.OutputPath))
        {
            request.OutputDestination.OutputPath = CreateDefaultOutputPath(request);
        }

        request.OutputDestination.SaveToFile = true;
    }

    private async Task<IReadOnlyList<ReportOutputGroup>> BuildSplitGroupsAsync(
        ReportOutputRequest request,
        object dataSource,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SplitKey)
            || !string.Equals(request.OutputFormat, "PDF", StringComparison.OrdinalIgnoreCase))
        {
            return await Task.FromResult<IReadOnlyList<ReportOutputGroup>>(
                new[] { new ReportOutputGroup { Key = null, DataSource = dataSource } });
        }

        if (dataSource is ReportDataSet dataSet)
        {
            var groups = dataSet.DetailRows
                .GroupBy(row => GetSplitKeyValue(row, request.SplitKey))
                .Select(group => new ReportOutputGroup
                {
                    Key = group.Key,
                    DataSource = CreateReportDataSet(group.ToList(), request.TemplateId)
                })
                .ToList();

            return await Task.FromResult<IReadOnlyList<ReportOutputGroup>>(groups);
        }

        if (dataSource is CnRecordSet recordSet)
        {
            var groupedRows = new Dictionary<string, List<Dictionary<string, object>>>(StringComparer.OrdinalIgnoreCase);
            using (recordSet)
            {
                while (!recordSet.Eof)
                {
                    var row = BuildRowDictionary(recordSet.CurrentRecord);
                    var key = GetSplitKeyValue(row, request.SplitKey);

                    if (!groupedRows.TryGetValue(key, out var rows))
                    {
                        rows = new List<Dictionary<string, object>>();
                        groupedRows[key] = rows;
                    }

                    rows.Add(row);

                    if (!recordSet.Next())
                        break;
                }
            }

            var groups = groupedRows
                .Select(group => new ReportOutputGroup
                {
                    Key = group.Key,
                    DataSource = CreateReportDataSet(group.Value, request.TemplateId)
                })
                .ToList();

            return await Task.FromResult<IReadOnlyList<ReportOutputGroup>>(groups);
        }

        if (dataSource is IEnumerable enumerable && dataSource is not string)
        {
            var groupedRows = new Dictionary<string, List<Dictionary<string, object>>>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in enumerable.Cast<object>())
            {
                var row = BuildRowDictionary(item);
                var key = GetSplitKeyValue(row, request.SplitKey);

                if (!groupedRows.TryGetValue(key, out var rows))
                {
                    rows = new List<Dictionary<string, object>>();
                    groupedRows[key] = rows;
                }

                rows.Add(row);
            }

            var groups = groupedRows
                .Select(group => new ReportOutputGroup
                {
                    Key = group.Key,
                    DataSource = CreateReportDataSet(group.Value, request.TemplateId)
                })
                .ToList();

            return await Task.FromResult<IReadOnlyList<ReportOutputGroup>>(groups);
        }

        return await Task.FromResult<IReadOnlyList<ReportOutputGroup>>(
            new[] { new ReportOutputGroup { Key = null, DataSource = dataSource } });
    }

    private static ReportDataSet CreateReportDataSet(
        IEnumerable<Dictionary<string, object>> detailRows,
        string? templateId)
    {
        return new ReportDataSet
        {
            TemplateId = templateId ?? string.Empty,
            HeaderData = new Dictionary<string, object>(),
            SummaryData = new Dictionary<string, object>(),
            DetailRows = detailRows
                .Select(row => new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase))
                .ToList()
        };
    }

    private static Dictionary<string, object> BuildRowDictionary(object? item)
    {
        var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (item == null)
            return row;

        if (item is IDictionary<string, object> dictionary)
        {
            foreach (var pair in dictionary)
            {
                row[pair.Key] = pair.Value ?? DBNull.Value;
            }

            return row;
        }

        if (item is IReadOnlyDictionary<string, object> readOnlyDictionary)
        {
            foreach (var pair in readOnlyDictionary)
            {
                row[pair.Key] = pair.Value ?? DBNull.Value;
            }

            return row;
        }

        var type = item.GetType();
        foreach (var property in type.GetProperties(System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
        {
            row[property.Name] = property.GetValue(item) ?? DBNull.Value;
        }

        return row;
    }

    private static string GetSplitKeyValue(IReadOnlyDictionary<string, object> row, string propertyName)
    {
        if (row == null || string.IsNullOrWhiteSpace(propertyName))
            return string.Empty;

        if (row.TryGetValue(propertyName, out var value))
            return value is DBNull ? string.Empty : value?.ToString() ?? string.Empty;

        foreach (var key in row.Keys)
        {
            if (string.Equals(key, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                var matchingValue = row[key];
                return matchingValue is DBNull ? string.Empty : matchingValue?.ToString() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private ReportOutputRequest CreateGroupRequest(
        ReportOutputRequest request,
        ReportOutputGroup group,
        string outputPath)
    {
        var dataRequest = request.DataRequest;
        return new ReportOutputRequest
        {
            ExecutionId = request.ExecutionId,
            JobExecutionId = request.JobExecutionId,
            ProcessDate = request.ProcessDate,
            ReportDataId = request.ReportDataId,
            TemplateId = request.TemplateId,
            OutputFormat = request.OutputFormat,
            SplitKey = request.SplitKey,
            DataRequest = new ReportDataRequest
            {
                Mode = dataRequest?.Mode ?? request.DataRequest?.Mode ?? ReportDataMode.Buffered,
                SourceData = group.DataSource,
                QueryParameters = dataRequest?.QueryParameters != null
                    ? new Dictionary<string, object>(dataRequest.QueryParameters)
                    : new Dictionary<string, object>()
            },
            OutputDestination = new ReportOutputDestinationRequest
            {
                OutputPath = outputPath,
                SaveToFile = request.OutputDestination?.SaveToFile ?? true,
                PrinterName = request.OutputDestination?.PrinterName ?? string.Empty
            }
        };
    }

    private string BuildOutputPathForGroup(
        ReportOutputRequest request,
        ReportOutputGroup group,
        int index)
    {
        if (string.IsNullOrWhiteSpace(group.Key)
            || !string.Equals(request.OutputFormat, "PDF", StringComparison.OrdinalIgnoreCase))
        {
            return request.OutputDestination?.OutputPath ?? CreateDefaultOutputPath(request);
        }

        return BuildUniqueOutputPath(
            request.OutputDestination?.OutputPath ?? CreateDefaultOutputPath(request),
            group.Key,
            index);
    }

    private string BuildUniqueOutputPath(string basePath, string? splitKeyValue, int index)
    {
        var directory = Path.GetDirectoryName(basePath) ?? Directory.GetCurrentDirectory();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(basePath);
        var extension = Path.GetExtension(basePath);
        var safeSplitKey = SanitizeFileName(splitKeyValue);

        for (var attempt = 0; attempt < 10; attempt++)
        {
            var candidate = Path.Combine(directory, $"{fileNameWithoutExtension}-{safeSplitKey}-{index}{extension}");
            if (!File.Exists(candidate))
                return candidate;

            Thread.Sleep(500);
            index++;
        }

        return Path.Combine(directory, $"{fileNameWithoutExtension}-{safeSplitKey}-{index}{extension}");
    }

    private static string GetPropertyValue(object item, string propertyName)
    {
        if (item == null)
            return string.Empty;

        var type = item.GetType();
        var property = type.GetProperty(
            propertyName,
            System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        return property?.GetValue(item)?.ToString() ?? string.Empty;
    }

    private static string SanitizeFileName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "all";

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        return sanitized.Length > 60 ? sanitized[..60] : sanitized;
    }

    private string CreateDefaultOutputPath(ReportOutputRequest request)
    {
        var jobExecutionId = string.IsNullOrWhiteSpace(request.JobExecutionId)
            ? request.ExecutionId
            : request.JobExecutionId;
        var templateId = string.IsNullOrWhiteSpace(request.TemplateId) ? "report" : request.TemplateId;

        if (_filePathProvider != null)
            return _filePathProvider.GetOutputFilePath(jobExecutionId, templateId);

        var outputDir = Path.Combine("output", "reports", jobExecutionId, templateId);
        Directory.CreateDirectory(outputDir);
        var fileName = $"{jobExecutionId}_{templateId}.pdf";
        return Path.Combine(outputDir, fileName);
    }

    private void ValidateRequest(ReportOutputRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.ExecutionId))
            throw new ArgumentException("ExecutionId は必須です。");

        if (string.IsNullOrWhiteSpace(request.TemplateId))
            throw new ArgumentException("TemplateId は必須です。");

        if (request.DataRequest == null)
            throw new ArgumentException("DataRequest は必須です。");

        if (request.OutputDestination == null)
            throw new ArgumentException("OutputDestination は必須です。");

        if (string.IsNullOrWhiteSpace(request.OutputDestination.OutputPath))
            throw new ArgumentException("OutputPath は必須です。");
    }

    private async Task<ReportTemplate> ValidateTemplate(
        string templateId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("テンプレート検証開始: {TemplateId}", templateId);

        var template = await _templateResolver.ResolveAsync(templateId, cancellationToken);

        if (template is null)
        {
            throw new FileNotFoundException($"テンプレート '{templateId}' が見つかりません。", templateId);
        }

        if (!template.IsActive)
        {
            _logger.LogError("テンプレートが無効です: {TemplateId}", templateId);
            throw new InvalidOperationException($"テンプレート '{templateId}' は無効です。");
        }

        _logger.LogDebug("テンプレート検証完了: {TemplateId}", templateId);
        return template;
    }

    private async Task<object> BuildData(
        ReportOutputRequest request,
        ReportTemplate template,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("データ組立開始 (Mode={Mode})", request.DataRequest?.Mode ?? ReportDataMode.Buffered);

        var dataSource = await _dataBuilder.BuildDataAsync(
            request.DataRequest!,
            template,
            cancellationToken);

        _logger.LogDebug("データ組立完了");
        return dataSource;
    }

    private async Task<GeneratedReport> GenerateReport(
        ReportOutputRequest request,
        ReportTemplate template,
        object dataSource,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("帳票生成開始");

        var generateRequest = new GenerateReportRequest
        {
            ExecutionId = request.ExecutionId,
            ReportDataId = request.ReportDataId,
            Template = template,
            DataSource = dataSource,
            Mode = request.DataRequest?.Mode ?? ReportDataMode.Buffered,
            OutputFilePath = request.OutputDestination?.OutputPath ?? string.Empty,
            OutputFormat = request.OutputFormat,
            HeaderData = new Dictionary<string, object>(),
            SummaryData = new Dictionary<string, object>()
        };

        var generatedReport = await _generator.GenerateReportAsync(generateRequest, cancellationToken);

        _logger.LogDebug("帳票生成完了");
        return generatedReport;
    }

    private async Task<bool> RouteOutput(
        ReportOutputRequest request,
        GeneratedReport generatedReport,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("出力先ルーティング開始 (OutputPath={OutputPath})",
            request.OutputDestination?.OutputPath ?? string.Empty);

        try
        {
            // 出力先パスの妥当性を検証
            await _outputDestination.ValidateOutputPathAsync(
                request.OutputDestination?.OutputPath ?? string.Empty,
                cancellationToken);

            // 生成済みファイルを出力先に送出
            var success = await _outputDestination.SendAsync(
                generatedReport.OutputFilePath ?? string.Empty,
                request.OutputDestination?.OutputPath ?? string.Empty,
                cancellationToken);

            _logger.LogDebug("出力先ルーティング完了 (Success={Success})", success);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "出力先ルーティングエラー");
            return false;
        }
    }

    private sealed class ReportOutputGroup
    {
        public string? Key { get; set; }
        public object DataSource { get; set; } = new object();
    }

    private ReportExecutionStatus MapExecutionStatus(string status)
    {
        return status switch
        {
            ReportExecutionStatuses.Running => ReportExecutionStatus.Running,
            ReportExecutionStatuses.Succeeded => ReportExecutionStatus.Completed,
            ReportExecutionStatuses.PartialOutput => ReportExecutionStatus.PartiallyCompleted,
            ReportExecutionStatuses.Failed => ReportExecutionStatus.Failed,
            _ => ReportExecutionStatus.Pending
        };
    }
}




