using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Batch.Framework;
using UnifiedAccount.Batch.Jobs.Report;

namespace UnifiedAccount.Batch.Workflows;

/// <summary>
/// サンプルワークフロー
/// </summary>
public class SampleWorkflow : JobBase
{
    public SampleWorkflow(IJobRunner jobs, ITransactionCoordinator transactionCoordinator, ILoggerFactory loggerFactory)
        : base(transactionCoordinator, loggerFactory, jobs)
    {
    }

    public override string JobId => "SampleWorkflow";
    public override string Description => "サンプルワークフロー";

    /// <summary>
    /// ワークフロー本体を実行する。
    /// </summary>
    /// <param name="ctx">ジョブコンテキストを指定する。</param>
    /// <param name="ct">キャンセルトークンを指定する。</param>
    /// <returns>子ジョブの結果を集約した実行結果。</returns>
    /// <remarks>
    /// トランザクションは <see cref="JobBase"/> が管理する。ワークフローが最外側のジョブとして
    /// トランザクションを開始するため、以下で起動する子ジョブは同一トランザクションへ参加し、
    /// ワークフロー全体が 1 つの境界となる。子ジョブ側でコミットは行われない。
    /// </remarks>
    protected override async Task<JobResult> ExecuteCoreAsync(JobContext ctx, CancellationToken ct)
    {
        var reportParam = ctx.GetParameter("ReportCode", string.Empty);
        var reportCodes = string.IsNullOrWhiteSpace(reportParam)
            ? new List<string>()
            : reportParam.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

        if (!reportCodes.Any())
        {
            Logger.LogInformation("SampleWorkflow: ReportCode パラメータが空のため処理なし");
            return JobResult.Ok(message: "対象銀行なし");
        }

        var results = new List<JobResult>();

        foreach (var reportCode in reportCodes)
        {
            try
            {
                ctx.Parameters["ReportCode"] = reportCode;
                Logger.LogInformation("SampleWorkflow: 帳票作成開始 ReportCode={ReportCode}", reportCode);

                // 子ジョブは ctx を共有するため、ワークフローが開始したトランザクションへ参加する。
                var result = await JobRunner.RunAsync<SampleReport>(ctx, ct);
                results.Add(result);

                if (result.Success)
                {
                    Logger.LogInformation("SampleWorkflow: 帳票作成成功 ReportCode={ReportCode}", reportCode);
                }
                else
                {
                    Logger.LogWarning("SampleWorkflow: 帳票作成失敗 ReportCode={ReportCode} Message={Msg}", reportCode, result.Message);
                }
            }
            catch (Exception ex)
            {
                // 1 件の失敗で全体を打ち切らず、残りの帳票を継続して作成する。
                // 集約結果が失敗となった場合はワークフロー全体がロールバックされる。
                Logger.LogWarning(ex, "SampleWorkflow: 帳票作成例外 ReportCode={ReportCode}", reportCode);
                results.Add(JobResult.Fail(ex.Message));
            }
        }

        var workflowResult = new WorkflowResult(results);
        return workflowResult.ToJobResult();
    }
}
