using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Batch.Framework;

/// <summary>
/// ジョブ実行のトランザクション境界を管理する。
/// </summary>
public class TransactionCoordinator : ITransactionCoordinator
{
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<TransactionCoordinator> _logger;

    /// <summary>
    /// トランザクション管理サービスを初期化する。
    /// </summary>
    /// <param name="logger">ロガーを指定する。</param>
    public TransactionCoordinator(ILogger<TransactionCoordinator> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<JobResult> RunJobAsync(
        JobContext context,
        Func<CancellationToken, Task<JobResult>> body,
        CancellationToken ct = default)
    {
        var db = context.Services.GetService<AppDbContext>();

        // 非リレーショナルプロバイダ（単体テストのインメモリ等）はトランザクションを持たないため、本体のみを実行する。
        if (db is null || !db.Database.IsRelational())
        {
            return await body(ct);
        }

        // 参加: 親ワークフローが開始済みのトランザクションを共有する。
        // 閉じるのは開始した側の責務であるため、ここではコミットもロールバックもしない。
        if (context.WorkflowTransaction is not null)
        {
            EnlistIfNeeded(db, context.WorkflowTransaction);
            return await body(ct);
        }

        // 開始: 自分が開始したトランザクションは自分が閉じる。
        // 単独起動したジョブはこの経路を通り、ジョブ単位がトランザクション境界となる。
        // ワークフローも 1 つのジョブとしてこの経路を通るため、配下の子ジョブは上の「参加」側へ落ち、
        // 結果としてワークフロー単位が境界となる。
        var transaction = await BeginTransactionAsync(db, ct);
        context.WorkflowTransaction = transaction;
        _logger.LogInformation(
            "TransactionCoordinator: transaction started JobExecutionId={JobExecutionId}",
            context.JobExecutionId);

        try
        {
            var result = await body(ct);

            // 業務条件による打ち切り（Success = false）は部分反映を残さないためロールバックする。
            // ロールバック対象外としたいデータ（帳票出力元データなど）は、
            // 本トランザクションとは別の DbContext で永続化すること。
            if (result.Success)
            {
                await transaction.CommitAsync(ct);
                _logger.LogInformation(
                    "TransactionCoordinator: transaction committed JobExecutionId={JobExecutionId}",
                    context.JobExecutionId);
            }
            else
            {
                await transaction.RollbackAsync(ct);
                _logger.LogWarning(
                    "TransactionCoordinator: transaction rolled back (business failure) JobExecutionId={JobExecutionId}",
                    context.JobExecutionId);
            }

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            _logger.LogWarning(
                "TransactionCoordinator: transaction rolled back (exception) JobExecutionId={JobExecutionId}",
                context.JobExecutionId);
            throw;
        }
        finally
        {
            // 開始者としての責務を終えたので、参加用の共有参照を解放する。
            context.WorkflowTransaction = null;
            await transaction.DisposeAsync();
        }
    }

    /// <summary>
    /// 指定のトランザクションが DbContext へ未設定の場合に限り紐付ける。
    /// </summary>
    /// <param name="db">対象の DbContext を指定する。</param>
    /// <param name="transaction">紐付けるトランザクションを指定する。</param>
    private static void EnlistIfNeeded(AppDbContext db, DbTransaction transaction)
    {
        // 同一スコープでは DbContext が使い回されるため、既に同じトランザクションへ参加済みのことがある。
        // その場合に UseTransaction を呼び直す必要はない。
        if (db.Database.CurrentTransaction?.GetDbTransaction() == transaction)
        {
            return;
        }

        db.Database.UseTransaction(transaction);
    }

    /// <summary>
    /// DbContext の接続上でトランザクションを開始する。
    /// </summary>
    /// <param name="db">対象の DbContext を指定する。</param>
    /// <param name="ct">キャンセルトークンを指定する。</param>
    /// <returns>開始したトランザクション。</returns>
    private static async Task<DbTransaction> BeginTransactionAsync(AppDbContext db, CancellationToken ct)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(ct);
        }

        var transaction = await connection.BeginTransactionAsync(ct);
        db.Database.UseTransaction(transaction);
        return transaction;
    }
}
