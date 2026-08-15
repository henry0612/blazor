using Microsoft.Extensions.Logging;

namespace UnifiedAccount.Batch.Framework;

/// <summary>
/// バッチジョブ/ワークフロー共通の基底クラス。
/// </summary>
/// <remarks>
/// トランザクション境界は本クラスが <see cref="ITransactionCoordinator"/> へ委譲するため、
/// 派生クラスの <see cref="ExecuteCoreAsync"/> ではトランザクションを開始も終了もしない。
/// 単独起動時はジョブ単位、ワークフローから呼ばれた場合はワークフロー単位が境界となる。
/// </remarks>
public abstract class JobBase : IJob
{
    /// <summary>
    /// トランザクション管理サービスを保持する。
    /// </summary>
    protected readonly ITransactionCoordinator TransactionCoordinator;

    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// ジョブ実行サービスを保持する。
    /// </summary>
    protected readonly IJobRunner JobRunner;

    /// <summary>
    /// 基底クラスを初期化する。
    /// </summary>
    /// <param name="transactionCoordinator">トランザクション管理サービスを指定する。</param>
    /// <param name="loggerFactory">ロガーファクトリを指定する。</param>
    /// <param name="jobRunner">ジョブ実行サービスを指定する。</param>
    protected JobBase(ITransactionCoordinator transactionCoordinator, ILoggerFactory loggerFactory, IJobRunner jobRunner)
    {
        TransactionCoordinator = transactionCoordinator;
        Logger = loggerFactory.CreateLogger(GetType());
        JobRunner = jobRunner;
    }

    /// <summary>
    /// ジョブID（例: "BAT-KOZ024"）
    /// </summary>
    public abstract string JobId { get; }

    /// <summary>
    /// ジョブ説明
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// ジョブを実行する。トランザクション境界の管理のみを行い、業務処理は
    /// <see cref="ExecuteCoreAsync"/> へ委譲する。
    /// </summary>
    /// <param name="context">ジョブコンテキストを指定する。</param>
    /// <param name="ct">キャンセルトークンを指定する。</param>
    /// <returns>ジョブの実行結果。</returns>
    public Task<JobResult> ExecuteAsync(JobContext context, CancellationToken ct = default)
    {
        return TransactionCoordinator.RunJobAsync(context, token => ExecuteCoreAsync(context, token), ct);
    }

    /// <summary>
    /// ジョブ本体を実行する。派生クラスで業務処理を実装する。
    /// </summary>
    /// <param name="context">ジョブコンテキストを指定する。</param>
    /// <param name="ct">キャンセルトークンを指定する。</param>
    /// <returns>ジョブの実行結果。</returns>
    /// <remarks>
    /// 本メソッドの呼び出し時点でトランザクションは開始済みである。
    /// <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(CancellationToken)"/> や
    /// <c>ExecuteUpdateAsync</c> の結果は、本メソッドが正常終了し
    /// <see cref="JobResult.Success"/> が true の場合にのみコミットされる。
    /// </remarks>
    protected abstract Task<JobResult> ExecuteCoreAsync(JobContext context, CancellationToken ct);
}
