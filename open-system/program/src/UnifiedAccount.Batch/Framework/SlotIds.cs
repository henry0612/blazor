namespace UnifiedAccount.Batch.Framework;

/// <summary>
/// ワークフロー排他用のスロットID定数
/// 実運用では DB の `BatchParameters` レコード ID を参照するよう設定する。
/// テスト時は JobContext.Parameters 経由で具体的な ID を渡すか、テスト用ガードを使用する。
/// </summary>
public static class SlotIds
{
    public const long MorningBatch = 1;
    public const long EveningBatch = 2;
    public const long FepBatch = 3;
    public const long KanjiImport = 4;
    public const long Billing = 5;
    public const long SameDay = 6;
}
