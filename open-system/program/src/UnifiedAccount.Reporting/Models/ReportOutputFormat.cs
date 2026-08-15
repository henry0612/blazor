namespace UnifiedAccount.Reporting.Models;

/// <summary>
/// 帳票出力形式
/// </summary>
public enum ReportOutputFormat
{
    /// <summary>
    /// CSV 出力 (エンジン不要)
    /// </summary>
    Csv,
    /// <summary>
    /// PDF 出力 (シーオーリポーツ帳票クリエータ等のエンジンが必要)
    /// </summary>
    Pdf
}

