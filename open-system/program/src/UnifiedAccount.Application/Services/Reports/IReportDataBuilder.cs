using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Domain.Interfaces.Reports;

namespace UnifiedAccount.Application.Services.Reports;

/// <summary>
/// 帳票テンプレートに対応するデータを取得し、バインド用データ構造を組み立てる（ステップ3）。
/// Buffered/Streamed デュアルモード対応。
/// COBOL移行元: SYMRTN (データ組立・検証ロジック)
/// </summary>
public interface IReportDataBuilder
{
    /// <summary>
    /// テンプレートに必要なデータを組立てる。
    /// Mode が Buffered の場合は ReportDataSet (List&lt;Dict&gt;) を返却。
    /// Mode が Streamed の場合は CnRecordSet を返却。
    /// </summary>
    Task<object> BuildDataAsync(
        ReportDataRequest dataRequest,
        ReportTemplate template,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Buffered モード時のデータ検証（型整合・必須項目チェック）。
    /// </summary>
    Task<ValidationResult> ValidateDataAsync(
        ReportDataSet dataSet,
        ReportTemplate template,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// データ検証結果。
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// 有効を取得または設定する。
    /// </summary>
    public bool IsValid { get; set; }
    /// <summary>
    /// エラー一覧を取得または設定する。
    /// </summary>
    public List<string> Errors { get; set; }
    /// <summary>
    /// 警告一覧を取得または設定する。
    /// </summary>
    public List<string> Warnings { get; set; }

    public ValidationResult()
    {
        Errors = new List<string>();
        Warnings = new List<string>();
    }
}

