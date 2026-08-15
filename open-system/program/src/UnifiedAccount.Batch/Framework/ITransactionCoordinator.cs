namespace UnifiedAccount.Batch.Framework;

/// <summary>
/// ジョブ実行のトランザクション境界を管理するサービス。
/// </summary>
/// <remarks>
/// トランザクションの所有権は「開始した側が閉じる」に統一する。
/// 単独起動したジョブは自分でトランザクションを開始し、自分でコミットまたはロールバックする。
/// ワークフローから呼ばれた子ジョブは、親が開始したトランザクションへ参加するだけで閉じない。
/// これによりジョブ単位／ワークフロー単位のどちらの境界も同一の仕組みで成立する。
/// </remarks>
public interface ITransactionCoordinator
{
    /// <summary>
    /// ジョブ本体をトランザクション管理下で実行する。
    /// </summary>
    /// <param name="context">ジョブコンテキストを指定する。</param>
    /// <param name="body">トランザクション内で実行するジョブ本体を指定する。</param>
    /// <param name="ct">キャンセルトークンを指定する。</param>
    /// <returns>ジョブ本体が返した実行結果。</returns>
    /// <remarks>
    /// <paramref name="context"/> の <see cref="JobContext.WorkflowTransaction"/> が未設定の場合は
    /// 新しいトランザクションを開始し、本メソッドの終了時に
    /// <see cref="JobResult.Success"/> が true ならコミット、false または例外送出ならロールバックする。
    /// 既に設定されている場合は参加するだけで、コミットもロールバックも行わない。
    /// 非リレーショナルプロバイダ（単体テストのインメモリ等）ではトランザクションを扱わず本体のみを実行する。
    /// </remarks>
    Task<JobResult> RunJobAsync(
        JobContext context,
        Func<CancellationToken, Task<JobResult>> body,
        CancellationToken ct = default);
}
