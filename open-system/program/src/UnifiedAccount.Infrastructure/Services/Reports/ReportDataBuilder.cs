using System.Reflection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Domain.Interfaces.Reports;
using UnifiedAccount.Application.Services.Reports;

namespace UnifiedAccount.Infrastructure.Services.Reports;

/// <summary>
/// 帳票テンプレートに対応するデータを取得し、バインド用データ構造を組み立てる（ステップ3）。
/// Buffered/Streamed デュアルモード対応。
/// COBOL移行元: SYMRTN (データ組立・検証ロジック)
/// </summary>
public class ReportDataBuilder : IReportDataBuilder
{
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<ReportDataBuilder> _logger;
    /// <summary>
    /// Auto モードで Buffered と Streamed を切り替える閾値。
    /// </summary>
    private const int BufferedThresholdRows = 10_000;

    public ReportDataBuilder(ILogger<ReportDataBuilder> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// テンプレートに必要なデータを組立てる。
    /// Mode が Buffered の場合は ReportDataSet を返却。
    /// Mode が Streamed の場合は CnRecordSet を返却。
    /// Mode が Auto の場合は行数に応じて判定（≤10k → Buffered、>10k → Streamed）
    /// </summary>
    /// <param name="dataRequest">データリクエストを指定する。</param>
    /// <param name="template">テンプレートを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task<object> BuildDataAsync(
        ReportDataRequest dataRequest,
        ReportTemplate template,
        CancellationToken cancellationToken = default)
    {
        if (dataRequest == null)
            throw new ArgumentNullException(nameof(dataRequest));

        if (template == null)
            throw new ArgumentNullException(nameof(template));

        var mode = dataRequest.Mode;

        // Auto モード時は行数推定で判定
        if (mode == ReportDataMode.Auto)
        {
            mode = EstimateMode(dataRequest);
            _logger.LogInformation("Auto モード判定: {SelectedMode}", mode);
        }

        if (mode == ReportDataMode.Buffered)
        {
            return await BuildBufferedDataAsync(dataRequest, template, cancellationToken);
        }
        else if (mode == ReportDataMode.Streamed)
        {
            return await BuildStreamedDataAsync(dataRequest, template, cancellationToken);
        }

        throw new InvalidOperationException($"未対応のモード: {mode}");
    }

    /// <summary>
    /// Buffered モード時のデータ検証。
    /// </summary>
    /// <param name="dataSet">データ集合を指定する。</param>
    /// <param name="template">テンプレートを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task<ValidationResult> ValidateDataAsync(
        ReportDataSet dataSet,
        ReportTemplate template,
        CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        if (dataSet == null)
        {
            result.IsValid = false;
            result.Errors.Add("データセットが null です。");
            return result;
        }

        // 必須フィールド確認
        foreach (var requiredField in template.RequiredFields)
        {
            if (!dataSet.HeaderData.ContainsKey(requiredField))
            {
                result.IsValid = false;
                result.Errors.Add($"必須ヘッダフィールド '{requiredField}' が不足しています。");
            }

            // 各行に必須フィールドがあるか確認
            foreach (var row in dataSet.DetailRows)
            {
                if (!row.ContainsKey(requiredField) && template.DetailFields.Contains(requiredField))
                {
                    result.Warnings.Add($"フィールド '{requiredField}' が行データに不足しています。");
                }
            }
        }

        // 集計データ確認
        if (dataSet.SummaryData == null)
        {
            result.Warnings.Add("集計データが空です。");
        }

        await Task.CompletedTask;
        return result;
    }

    private async Task<object> BuildBufferedDataAsync(
        ReportDataRequest dataRequest,
        ReportTemplate template,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buffered モードでデータ組立開始");

        var dataSet = new ReportDataSet
        {
            TemplateId = template.TemplateId
        };

        // ヘッダデータを抽出
        ExtractHeaderData(dataRequest.SourceData, dataSet.HeaderData, template.RequiredFields);

        // 業務ジョブでは SourceData を必須とし、明細行をそこから組み立てる。
        if (!TryExtractDetailRowsFromSourceData(dataRequest.SourceData, template.DetailFields, out var sourceRows))
            throw new ArgumentException("SourceData は必須です。明細行データを指定してください。", nameof(dataRequest.SourceData));

        dataSet.DetailRows.AddRange(sourceRows);

        // 集計データを計算
        CalculateSummaryData(dataSet);

        // データ検証
        var validationResult = await ValidateDataAsync(dataSet, template, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("データ検証エラー: {Errors}", string.Join(", ", validationResult.Errors));
            throw new InvalidOperationException($"データ検証に失敗しました: {string.Join(", ", validationResult.Errors)}");
        }

        _logger.LogInformation("Buffered モードでデータ組立完了: {RowCount} 行", dataSet.DetailRows.Count);
        return dataSet;
    }

    private async Task<object> BuildStreamedDataAsync(
        ReportDataRequest dataRequest,
        ReportTemplate template,
        CancellationToken cancellationToken)
    {
        _ = template;
        _ = cancellationToken;

        _logger.LogInformation("Streamed モードでデータプロバイダ生成開始");

        // 業務ジョブでは SourceData を必須とする。
        var detailEnumerable = TryExtractDetailEnumerableFromSourceData(dataRequest.SourceData);
        if (detailEnumerable == null)
            throw new ArgumentException("SourceData は必須です。列挙可能な明細行データを指定してください。", nameof(dataRequest.SourceData));

        // EfCoreRecordSet でラップ
        var recordSet = new EfCoreRecordSet(detailEnumerable.Cast<object>());

        _logger.LogInformation("Streamed モードでレコードセット生成完了 (RecordCount: {RecordCount})", recordSet.RecordCount);

        await Task.CompletedTask;
        return recordSet;
    }

    private void ExtractHeaderData(
        object? sourceData,
        Dictionary<string, object> headerData,
        ICollection<string> requiredFields)
    {
        if (sourceData == null)
            return;

        // sourceData が Dictionary の場合
        if (sourceData is Dictionary<string, object> sourceDict)
        {
            foreach (var field in requiredFields)
            {
                if (sourceDict.TryGetValue(field, out var value))
                {
                    headerData[field] = value;
                }
            }
        }
        else
        {
            // オブジェクトのプロパティからリフレクション抽出
            var type = sourceData.GetType();
            foreach (var field in requiredFields)
            {
                var property = type.GetProperty(field);
                if (property != null)
                {
                    var value = property.GetValue(sourceData);
                    headerData[field] = value ?? DBNull.Value;
                }
            }
        }
    }

    private static bool TryExtractDetailRowsFromSourceData(
        object? sourceData,
        ICollection<string> detailFields,
        out List<Dictionary<string, object>> rows)
    {
        rows = new List<Dictionary<string, object>>();
        if (sourceData == null || sourceData is string)
            return false;

        if (sourceData is IEnumerable<Dictionary<string, object>> sourceRows)
        {
            rows = sourceRows
                .Select(row => row.ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase))
                .ToList();
            return rows.Count > 0;
        }

        if (sourceData is IEnumerable<IDictionary<string, object>> dictRows)
        {
            rows = dictRows
                .Select(row => row.ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase))
                .ToList();
            return rows.Count > 0;
        }

        if (sourceData is not IEnumerable<object> objectRows)
            return false;

        foreach (var item in objectRows)
        {
            if (item is IDictionary<string, object> itemDictionary)
            {
                rows.Add(itemDictionary.ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase));
                continue;
            }

            var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var properties = item.GetType()
                .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead);

            if (detailFields.Count > 0)
            {
                properties = properties.Where(prop =>
                    detailFields.Any(field => string.Equals(field, prop.Name, StringComparison.OrdinalIgnoreCase)));
            }

            foreach (var property in properties)
                values[property.Name] = property.GetValue(item) ?? DBNull.Value;

            if (values.Count > 0)
                rows.Add(values);
        }

        return rows.Count > 0;
    }

    private static IEnumerable<object>? TryExtractDetailEnumerableFromSourceData(object? sourceData)
    {
        if (sourceData == null || sourceData is string)
            return null;

        if (sourceData is IEnumerable<object> objectEnumerable)
            return objectEnumerable;

        return null;
    }

    private void CalculateSummaryData(ReportDataSet dataSet)
    {
        // 集計処理（例: 行数、件数など）
        dataSet.SummaryData["TotalRows"] = dataSet.DetailRows.Count;
        dataSet.SummaryData["RecordCount"] = dataSet.DetailRows.Count;

        // TODO: テンプレートに応じた金額合計などの集計実装
        _logger.LogInformation("集計完了: 合計 {Count} 行", dataSet.DetailRows.Count);
    }

    private ReportDataMode EstimateMode(ReportDataRequest dataRequest)
    {
        if (dataRequest.SourceData == null || dataRequest.SourceData is string)
            throw new ArgumentException("SourceData は必須です。", nameof(dataRequest.SourceData));

        try
        {
            var estimatedCount = EstimateSourceDataCount(dataRequest.SourceData);
            if (estimatedCount == null)
            {
                _logger.LogInformation("Mode 推定: SourceData 件数不明のため Buffered を採用");
                return ReportDataMode.Buffered;
            }

            if (estimatedCount <= BufferedThresholdRows)
            {
                _logger.LogInformation("Mode 推定: SourceData {Count} 行 ≤ {Threshold} → Buffered",
                    estimatedCount, BufferedThresholdRows);
                return ReportDataMode.Buffered;
            }
            else
            {
                _logger.LogInformation("Mode 推定: SourceData {Count} 行 > {Threshold} → Streamed",
                    estimatedCount, BufferedThresholdRows);
                return ReportDataMode.Streamed;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Mode 推定エラー。デフォルト Buffered を採用");
            return ReportDataMode.Buffered;
        }
    }

    private static int? EstimateSourceDataCount(object sourceData)
    {
        if (sourceData is ICollection<object> objectCollection)
            return objectCollection.Count;

        if (sourceData is System.Collections.ICollection collection)
            return collection.Count;

        if (sourceData is Array array)
            return array.Length;

        return null;
    }
}

