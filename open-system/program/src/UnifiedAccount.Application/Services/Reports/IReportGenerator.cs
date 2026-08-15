using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Domain.Interfaces.Reports;

namespace UnifiedAccount.Application.Services.Reports;

/// <summary>
/// CoReports 帳票クリエータを使用して、PDF/PRN形式の帳票を生成する（ステップ4）。
/// Buffered/Streamed デュアルモード対応。
/// COBOL移行元: SYSGENE (PDF/PRN生成ロジック)
/// </summary>
public interface IReportGenerator
{
    /// <summary>
    /// Buffered または Streamed モードで帳票を生成。
    /// Buffered: ReportDataSet (List&lt;Dict&gt;) を受け入れ
    /// Streamed: CnRecordSet を受け入れ
    /// 返却: GeneratedReport (出力ファイルパス、ファイルサイズ、IsPartialOutput)
    /// </summary>
    Task<GeneratedReport> GenerateReportAsync(
        GenerateReportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 生成済みの帳票情報を照会。
    /// </summary>
    Task<GeneratedReport> GetGeneratedReportAsync(
        string executionId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 帳票生成リクエスト。
/// DataSource は ReportDataSet OR CnRecordSet
/// </summary>
public class GenerateReportRequest
{
    /// <summary>
    /// 実行IDを取得または設定する。
    /// </summary>
    public string ExecutionId { get; set; } = string.Empty;
    /// <summary>
    /// 帳票データIDを取得または設定する。
    /// </summary>
    public string? ReportDataId { get; set; }
    /// <summary>
    /// テンプレートを取得または設定する。
    /// </summary>
    public ReportTemplate Template { get; set; } = new();
    /// <summary>
    /// データソースを取得または設定する。
    /// </summary>
    public object DataSource { get; set; } = new object();
    /// <summary>
    /// モードを取得または設定する。
    /// </summary>
    public ReportDataMode Mode { get; set; }
    /// <summary>
    /// 出力ファイルパスを取得または設定する。
    /// </summary>
    public string OutputFilePath { get; set; } = string.Empty;
    /// <summary>
    /// 出力形式を取得または設定する。
    /// </summary>
    public string? OutputFormat { get; set; }
    /// <summary>
    /// ヘッダーデータを取得または設定する。
    /// </summary>
    public Dictionary<string, object>? HeaderData { get; set; }
    /// <summary>
    /// 集計データを取得または設定する。
    /// </summary>
    public Dictionary<string, object>? SummaryData { get; set; }
}

/// <summary>
/// Streamed モード中断例外。
/// 帳票出力中にDBコネクション喪失や CoReports エラーで途中終了した場合。
/// 出力は .incomplete ファイルに保存される。
/// </summary>
public class StreamingInterruptedException : Exception
{
    /// <summary>
    /// 有無一部出力を取得または設定する。
    /// </summary>
    public bool HasPartialOutput { get; set; }
    /// <summary>
    /// 未完了ファイルパスを取得または設定する。
    /// </summary>
    public string? IncompleteFilePath { get; set; }

    public StreamingInterruptedException(
        string message,
        bool hasPartialOutput = false,
        string? incompleteFilePath = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        HasPartialOutput = hasPartialOutput;
        IncompleteFilePath = incompleteFilePath;
    }
}

