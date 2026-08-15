using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 生成済みの帳票ファイル情報。
/// 出力先（PDF/PRN）、実行結果、エラー情報を保持。
/// </summary>
public class GeneratedReport
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
    /// テンプレートIDを取得または設定する。
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;
    /// <summary>
    /// 出力ファイルパスを取得または設定する。
    /// </summary>
    public string? OutputFilePath { get; set; }
    /// <summary>
    /// 出力形式を取得または設定する。
    /// </summary>
    public string OutputFormat { get; set; } = string.Empty;
    /// <summary>
    /// ファイルサイズバイト数を取得または設定する。
    /// </summary>
    public long FileSizeBytes { get; set; }
    /// <summary>
    /// 一部出力を取得または設定する。
    /// </summary>
    public bool IsPartialOutput { get; set; }
    /// <summary>
    /// 生成日時を取得または設定する。
    /// </summary>
    public DateTime GeneratedAt { get; set; }
    /// <summary>
    /// 生成例外を取得または設定する。
    /// </summary>
    public Exception? GenerationException { get; set; }

    public GeneratedReport()
    {
        GeneratedAt = LocalDateTimeProvider.Now;
        IsPartialOutput = false;
    }

    public bool IsSuccessful()
    {
        return !string.IsNullOrEmpty(OutputFilePath) && FileSizeBytes > 0;
    }

    public bool IsFailed()
    {
        return GenerationException != null;
    }
}
