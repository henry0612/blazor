using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Application.Services;

namespace UnifiedAccount.Batch.Framework;

/// <summary>
/// ジョブ実行のエントリーポイント
/// </summary>
public class JobRunner
{
    /// <summary>
    /// サービスプロバイダを保持する。
    /// </summary>
    private readonly IServiceProvider _serviceProvider;
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<JobRunner> _logger;
    /// <summary>
    /// 実行番号サービスを保持する。
    /// </summary>
    private readonly IExecutionNumberService _executionNumberService;

    public JobRunner(IServiceProvider serviceProvider, ILogger<JobRunner> logger, IExecutionNumberService executionNumberService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _executionNumberService = executionNumberService;
    }

    /// <summary>
    /// コマンドライン引数からジョブを実行する
    /// </summary>
    /// <param name="args">引数を指定する。</param>
    /// <returns>ジョブの終了コード</returns>
    public async Task<int> RunAsync(string[] args)
    {
        var jobId = GetArgValue(args, "--job") ?? GetArgValue(args, "-j");
        var dateStr = GetArgValue(args, "--date") ?? GetArgValue(args, "-d");
        var executionId = GetArgValue(args, "--execution-id");

        if (string.IsNullOrEmpty(jobId))
        {
            _logger.LogError("ジョブIDが指定されていません。Usage: --job <JobId> [--date <yyyy-MM-dd>] [--execution-id <JobExecutionId>]");
            return 1;
        }

        var processDate = string.IsNullOrEmpty(dateStr)
            ? DateOnly.FromDateTime(DateTime.Today)
            : DateOnly.Parse(dateStr);

        // 追加パラメータを収集
        var parameters = new Dictionary<string, string>();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].StartsWith("--param:"))
            {
                var key = args[i]["--param:".Length..];
                parameters[key] = args[i + 1];
            }
        }

        return await RunJobAsync(jobId, processDate, parameters, executionId);
    }

    /// <summary>
    /// 指定ジョブを実行
    /// </summary>
    /// <param name="jobId">ジョブIDを指定する。</param>
    /// <param name="processDate">処理日付を指定する。</param>
    /// <param name="parameters">パラメータ一覧を指定する。</param>
    /// <param name="executionId">実行IDを指定する。</param>
    public async Task<int> RunJobAsync(string jobId, DateOnly processDate, IDictionary<string, string>? parameters = null, string? executionId = null)
    {
        using var scope = _serviceProvider.CreateScope();
        var jobs = scope.ServiceProvider.GetServices<IJob>();
        var job = jobs.FirstOrDefault(j => j.JobId.Equals(jobId, StringComparison.OrdinalIgnoreCase));

        if (job == null)
        {
            _logger.LogError("ジョブ '{JobId}' が見つかりません。", jobId);
            return 1;
        }

        var jobExecutionId = executionId ?? _executionNumberService.GenerateJobExecutionId(job.JobId, processDate);
        if (executionId != null)
        {
            _logger.LogWarning("デバッグ用 JobExecutionId を使用: {JobExecutionId}", jobExecutionId);
        }
        var context = new JobContext(processDate, scope.ServiceProvider, parameters as Dictionary<string, string>, jobExecutionId);

        _logger.LogInformation(
            // 処理日は実行環境のカルチャに依存しない ISO 形式（yyyy-MM-dd）で出力する。
            // 既定書式では実行機のロケールにより 06/26/2026 等となり、証跡の判定基準が環境ごとに変わってしまう。
            "=== ジョブ開始: {JobId} ({Description}) 処理日: {ProcessDate:yyyy-MM-dd} JobExecutionId: {JobExecutionId} ===",
            job.JobId, job.Description, processDate, jobExecutionId);

        try
        {
            var result = await job.ExecuteAsync(context);

            if (result.Success)
            {
                _logger.LogInformation(
                    "=== ジョブ完了: {JobId} 読込:{Read} 書込:{Write} エラー:{Error} 時間:{Duration:F0}ms ===",
                    job.JobId, result.ReadCount, result.WriteCount, result.ErrorCount,
                    result.Duration?.TotalMilliseconds ?? 0);
                return 0;
            }
            else
            {
                _logger.LogError(
                    "=== ジョブ失敗: {JobId} メッセージ:{Message} 読込:{Read} エラー:{Error} ===",
                    job.JobId, result.Message, result.ReadCount, result.ErrorCount);
                return 8;
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== ジョブ異常終了: {JobId} ===", job.JobId);
            return 12;
        }
    }

    private static string? GetArgValue(string[] args, string key)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(key, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }
        return null;
    }
}




