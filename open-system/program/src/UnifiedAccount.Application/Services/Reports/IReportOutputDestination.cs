namespace UnifiedAccount.Application.Services.Reports;

/// <summary>
/// 帳票出力先インターフェース（ステップ5）
/// 生成された帳票をファイルシステム、プリンター等に出力
/// MVP: ファイルシステムのみ対応
/// </summary>
public interface IReportOutputDestination
{
    /// <summary>
    /// 帳票ファイルを出力先に送出
    /// </summary>
    /// <returns>出力成功フラグ</returns>
    Task<bool> SendAsync(
        string sourceFilePath,
        string outputPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ファイルコピー時にバリデーション＆権限確認
    /// </summary>
    Task ValidateOutputPathAsync(
        string outputPath,
        CancellationToken cancellationToken = default);
}

