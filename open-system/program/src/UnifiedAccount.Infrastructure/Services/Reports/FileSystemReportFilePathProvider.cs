using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnifiedAccount.Application.Services.Reports;
using UnifiedAccount.Infrastructure.Configuration;

namespace UnifiedAccount.Infrastructure.Services.Reports;

/// <summary>
/// ファイルシステムへの帳票出力ファイルパス生成実装。
/// 呼び出し時に出力先ディレクトリを作成し、ファイルパスを返す。
/// </summary>
public class FileSystemReportFilePathProvider : IReportFilePathProvider
{
    /// <summary>
    /// 設定一覧を保持する。
    /// </summary>
    private readonly ReportSettings _settings;
    /// <summary>
    /// 時刻プロバイダを保持する。
    /// </summary>
    private readonly TimeProvider _timeProvider;
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<FileSystemReportFilePathProvider> _logger;

    public FileSystemReportFilePathProvider(
        IOptions<ReportSettings> settings,
        TimeProvider timeProvider,
        ILogger<FileSystemReportFilePathProvider> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    /// <param name="jobExecutionId">ジョブ実行IDを指定する。</param>
    /// <param name="jobId">ジョブIDを指定する。</param>
    public string GetOutputFilePath(string jobExecutionId, string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobExecutionId))
            throw new ArgumentException("jobExecutionId は必須です。", nameof(jobExecutionId));
        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("jobId は必須です。", nameof(jobId));

        var timestamp = _timeProvider.GetLocalNow().ToString("yyyyMMddHHmmss");
        var extension = GetFileExtension(jobId);

        var outputDir = Path.Combine(
            _settings.OutputFolder,
            jobExecutionId,
            $"{jobId}_{timestamp}");

        Directory.CreateDirectory(outputDir);

        _logger.LogDebug(
            "帳票出力ディレクトリ作成: {Directory}",
            outputDir);

        var fileName = $"{jobExecutionId}_{jobId}_{timestamp}.{extension}";
        return Path.Combine(outputDir, fileName);
    }

    /// <summary>
    /// ジョブIDに対応する出力ファイル拡張子を返す。
    /// 現時点では全ジョブ PDF 固定。
    /// 将来: JobId 別の拡張子マッピングをここに追加する。
    /// </summary>
    protected virtual string GetFileExtension(string jobId) => "PDF";
}

