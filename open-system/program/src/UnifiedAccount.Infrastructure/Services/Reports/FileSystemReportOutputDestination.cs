using Microsoft.Extensions.Logging;
using UnifiedAccount.Application.Services.Reports;

namespace UnifiedAccount.Infrastructure.Services.Reports;

/// <summary>
/// ファイルシステム出力先実装（ステップ5）
/// 生成済み帳票をファイルシステムにコピー
/// </summary>
public class FileSystemReportOutputDestination : IReportOutputDestination
{
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<FileSystemReportOutputDestination> _logger;

    public FileSystemReportOutputDestination(
        ILogger<FileSystemReportOutputDestination> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 帳票ファイルをファイルシステムに出力
    /// </summary>
    /// <param name="sourceFilePath">ソースファイルパスを指定する。</param>
    /// <param name="outputPath">出力パスを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task<bool> SendAsync(
        string sourceFilePath,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 入力パス検証
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                _logger.LogWarning("出力元ファイルパスが空です");
                return false;
            }

            if (!File.Exists(sourceFilePath))
            {
                _logger.LogError("出力元ファイルが見つかりません: {FilePath}", sourceFilePath);
                throw new FileNotFoundException($"Source file not found: {sourceFilePath}");
            }

            // 出力先パス検証
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                _logger.LogWarning("出力先パスが空です");
                return false;
            }

            // 出力先ディレクトリを作成
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                _logger.LogInformation("出力先ディレクトリを作成: {Directory}", outputDir);
            }

            var fullSourcePath = Path.GetFullPath(sourceFilePath);
            var fullOutputPath = Path.GetFullPath(outputPath);

            if (string.Equals(fullSourcePath, fullOutputPath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("帳票出力スキップ（同一パス）: {OutputPath}", outputPath);
                return true;
            }

            // ファイルをコピー（既存ファイルは上書き）
            await Task.Run(() =>
            {
                File.Copy(fullSourcePath, fullOutputPath, overwrite: true);
            }, cancellationToken);

            var fileInfo = new FileInfo(fullOutputPath);
            _logger.LogInformation(
                "帳票出力成功: {OutputPath} ({FileSize} bytes)",
                outputPath, fileInfo.Length);

            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("帳票出力がキャンセルされました");
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "出力先への書き込み権限がありません: {OutputPath}", outputPath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "帳票出力エラー: {SourcePath} → {OutputPath}",
                sourceFilePath, outputPath);
            throw;
        }
    }

    /// <summary>
    /// 出力先パスの妥当性確認
    /// </summary>
    /// <param name="outputPath">出力パスを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task ValidateOutputPathAsync(
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new ArgumentException("出力先パスが空です", nameof(outputPath));
            }

            var outputDir = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(outputDir))
            {
                throw new ArgumentException("出力先ディレクトリが指定されていません", nameof(outputPath));
            }

            Directory.CreateDirectory(outputDir);

            // パスが有効な形式か確認
            var invalidChars = Path.GetInvalidPathChars();
            if (outputPath.Any(c => invalidChars.Contains(c)))
            {
                throw new ArgumentException(
                    "出力先パスに無効な文字が含まれています", nameof(outputPath));
            }

            _logger.LogInformation("出力先パス検証成功: {OutputPath}", outputPath);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "出力先パス検証エラー: {OutputPath}", outputPath);
            throw;
        }
    }
}




