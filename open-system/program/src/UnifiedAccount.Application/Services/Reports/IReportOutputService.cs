using UnifiedAccount.Domain.Entities.Reports;

namespace UnifiedAccount.Application.Services.Reports;

/// <summary>
/// 帳票出力の公開契約（アプリケーション入口）。
/// ステップ1～7: テンプレート検証 → データ組立 → 生成 → 出力 → 結果記録 → リトライまで統合。
/// 新規ジョブからは本契約を優先利用し、個別レンダラ実装への直接依存を避ける。
/// COBOL移行元: SYSMAIN (帳票出力主処理)
/// </summary>
public interface IReportOutputService
{
    /// <summary>
    /// 帳票出力リクエストを受け付け、テンプレート検証、データ組立、PDF/PRN生成、出力を行う。
    /// </summary>
    Task<ReportOutputResponse> OutputAsync(ReportOutputRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 帳票実行結果を照会する。
    /// </summary>
    Task<ReportExecutionResult> GetExecutionResultAsync(string executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 失敗した帳票出力をリトライする（.incomplete 処理済みの場合のみ）。
    /// </summary>
    Task<ReportOutputResponse> RetryAsync(string executionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 帳票出力が失敗した処理段階。
/// </summary>
public enum ReportOutputFailureKind
{
    None = 0,
    Validation = 1,
    TemplateNotFound = 2,
    DataBuildFailed = 3,
    GenerationFailed = 4,
    DestinationFailed = 5,
    Unexpected = 6
}

/// <summary>
/// 帳票出力リクエスト。
/// </summary>
public class ReportOutputRequest
{
    /// <summary>
    /// 帳票印刷単位の一意識別子（1印刷 = 1レコード）。呼び出し元が生成して渡す。
    /// </summary>
    public required string ExecutionId { get; set; }
    /// <summary>
    /// ジョブフロー識別子（JobContext.JobExecutionId）。バッチから呼ぶ場合に設定する。
    /// </summary>
    public string? JobExecutionId { get; set; }
    /// <summary>
    /// 処理日付を取得または設定する。
    /// </summary>
    public DateOnly ProcessDate { get; set; }
    /// <summary>
    /// 帳票データIDを取得または設定する。
    /// </summary>
    public string? ReportDataId { get; set; }
    /// <summary>
    /// テンプレートIDを取得または設定する。
    /// </summary>
    public required string TemplateId { get; set; }
    /// <summary>
    /// 出力形式を取得または設定する。
    /// </summary>
    public string OutputFormat { get; set; } = "PDF";
    /// <summary>
    /// 分割キーを取得または設定する。
    /// </summary>
    public string? SplitKey { get; set; }
    /// <summary>
    /// データリクエストを取得または設定する。
    /// </summary>
    public ReportDataRequest? DataRequest { get; set; }
    /// <summary>
    /// 出力出力先を取得または設定する。
    /// </summary>
    public ReportOutputDestinationRequest? OutputDestination { get; set; }
}

/// <summary>
/// データ組立リクエスト。
/// </summary>
public class ReportDataRequest
{
    /// <summary>
    /// ソースデータを取得または設定する。
    /// </summary>
    public object? SourceData { get; set; }
    /// <summary>
    /// 照会パラメータ一覧を取得または設定する。
    /// </summary>
    public Dictionary<string, object>? QueryParameters { get; set; }
    /// <summary>
    /// モードを取得または設定する。
    /// </summary>
    public ReportDataMode Mode { get; set; }
}

/// <summary>
/// 出力先指定リクエスト。
/// </summary>
public class ReportOutputDestinationRequest
{
    /// <summary>
    /// 出力パスを取得または設定する。
    /// </summary>
    public string? OutputPath { get; set; }
    /// <summary>
    /// 保存終了ファイルを取得または設定する。
    /// </summary>
    public bool SaveToFile { get; set; } = true;
    /// <summary>
    /// プリンタ名称を取得または設定する。
    /// </summary>
    public string? PrinterName { get; set; }
}

/// <summary>
/// 帳票出力レスポンス。
/// </summary>
public class ReportOutputResponse
{
    /// <summary>
    /// 成功可否を取得または設定する。
    /// </summary>
    public bool IsSuccessful { get; set; }
    /// <summary>
    /// 呼出側がメッセージ文字列を解析せずに終了経路を判定するための失敗種別。
    /// </summary>
    public ReportOutputFailureKind FailureKind { get; set; }
    /// <summary>
    /// 実行IDを取得または設定する。
    /// </summary>
    public string? ExecutionId { get; set; }
    /// <summary>
    /// 帳票データIDを取得または設定する。
    /// </summary>
    public string? ReportDataId { get; set; }
    /// <summary>
    /// 出力ファイルパスを取得または設定する。
    /// </summary>
    public string? OutputFilePath { get; set; }
    /// <summary>
    /// 生成帳票を取得または設定する。
    /// </summary>
    public GeneratedReport? GeneratedReport { get; set; }
    /// <summary>
    /// 検証エラー一覧を取得または設定する。
    /// </summary>
    public List<string> ValidationErrors { get; set; }

    public ReportOutputResponse()
    {
        ValidationErrors = new List<string>();
    }
}

/// <summary>
/// 帳票実行結果。
/// </summary>
public class ReportExecutionResult
{
    /// <summary>
    /// 実行IDを取得または設定する。
    /// </summary>
    public string? ExecutionId { get; set; }
    /// <summary>
    /// テンプレートIDを取得または設定する。
    /// </summary>
    public string? TemplateId { get; set; }
    /// <summary>
    /// ステータスを取得または設定する。
    /// </summary>
    public ReportExecutionStatus Status { get; set; }
    /// <summary>
    /// 実行日時を取得または設定する。
    /// </summary>
    public DateTime ExecutedAt { get; set; }
    /// <summary>
    /// リトライ件数を取得または設定する。
    /// </summary>
    public int RetryCount { get; set; }
    /// <summary>
    /// 一部出力を取得または設定する。
    /// </summary>
    public bool IsPartialOutput { get; set; }
    /// <summary>
    /// エラーメッセージを取得または設定する。
    /// </summary>
    public string? ErrorMessage { get; set; }
    /// <summary>
    /// 出力ファイルパスを取得または設定する。
    /// </summary>
    public string? OutputFilePath { get; set; }
}

/// <summary>
/// 帳票出力モード: Buffered（全データ保持）/ Streamed（逐次供給）
/// </summary>
public enum ReportDataMode
{
    Buffered = 0,
    Streamed = 1,
    Auto = 2
}

/// <summary>
/// 帳票実行ステータス。
/// </summary>
public enum ReportExecutionStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    PartiallyCompleted = 3,
    Failed = 4,
    Cancelled = 5
}


