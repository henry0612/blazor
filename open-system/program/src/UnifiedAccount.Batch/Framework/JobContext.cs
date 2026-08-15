using System.Data.Common;
using UnifiedAccount.Domain.Enums;

namespace UnifiedAccount.Batch.Framework;

/// <summary>
/// ジョブ実行コンテキスト (JCL PARM= 相当)
/// 振替処理では TransferRound / WithdrawalDay / IsUedaAfter を Parameters に設定して渡す。
/// </summary>
public class JobContext
{
    /// <summary>
    /// 処理日
    /// </summary>
    public DateOnly ProcessDate { get; }

    /// <summary>
    /// DIコンテナ
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// ジョブ実行番号
    /// </summary>
    public string JobExecutionId { get; }

    /// <summary>
    /// 追加パラメータ
    /// </summary>
    public IDictionary<string, string> Parameters { get; }

    /// <summary>
    /// ワークフローが開始したトランザクション。子ジョブがこれを再利用する。
    /// </summary>
    public DbTransaction? WorkflowTransaction { get; set; }

    public JobContext(DateOnly processDate, IServiceProvider services, IDictionary<string, string>? parameters = null, string? jobExecutionId = null)
    {
        ProcessDate = processDate;
        Services = services;
        Parameters = parameters ?? new Dictionary<string, string>();
        JobExecutionId = string.IsNullOrWhiteSpace(jobExecutionId)
            ? Guid.NewGuid().ToString("N")
            : jobExecutionId;
    }

    /// <summary>
    /// パラメータ取得 (キーなし時はデフォルト値)
    /// </summary>
    /// <param name="key">キーを指定する。</param>
    /// <param name="defaultValue">既定値を指定する。</param>
    public string GetParameter(string key, string defaultValue = "")
    {
        return Parameters.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// 振替回 (JCL RUNJOB → JB値相当)
    /// Parameters["TransferRound"] = "1"(JOB1) / "2"(JOB2) / "3"(JOB3)
    /// 未指定時は Round1。
    /// </summary>
    public TransferRound TransferRound =>
        Enum.TryParse<TransferRound>(GetParameter("TransferRound", "1"), out var round)
            ? round
            : TransferRound.Round1;

    /// <summary>
    /// 振替日 (JCL FURIKAEBI相当: 2, 9, 12, 16, 22, 26)
    /// Parameters["WithdrawalDay"] = "2" などの日付数字文字列。
    /// 未指定時は 0。
    /// </summary>
    public int WithdrawalDay =>
        int.TryParse(GetParameter("WithdrawalDay", "0"), out var day) ? day : 0;

    /// <summary>
    /// 上田ケーブル後処理フラグ (JCL UEDAGO相当)
    /// JOB3 かつ振替日26日のみ true。
    /// Parameters["IsUedaAfter"] = "true" で有効化。
    /// </summary>
    public bool IsUedaAfter =>
        GetParameter("IsUedaAfter", "false").Equals("true", StringComparison.OrdinalIgnoreCase);
}




