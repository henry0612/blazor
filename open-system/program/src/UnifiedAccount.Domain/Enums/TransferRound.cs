namespace UnifiedAccount.Domain.Enums;

/// <summary>
/// 振替回 (JCL RUNJOB パラメータ相当)
/// 同一月内で最大3回の引落し試行サイクルを表す。
/// COBOLの WPL-KUBUN / APL-KUBUN ('1'/'2'/'3') に対応。
/// </summary>
public enum TransferRound
{
    /// <summary>
    /// 第1回振替 (JOB1) — 新規引落し (初回)
    /// 対象振替日: 02, 09, 12, 22, 26日
    /// 前提条件: 制限なし
    /// </summary>
    Round1 = 1,

    /// <summary>
    /// 第2回振替 (JOB2) — 初回不能者の再引落し
    /// 対象振替日: 02, 09, 12, 16, 22, 26日 (16日はJOB2のみ)
    /// 前提条件: PreviousTransferRound = "0"(未処理) or "3"(JOB3済→次サイクル)
    /// </summary>
    Round2 = 2,

    /// <summary>
    /// 第3回振替 (JOB3) — 再振替 (最終引落し)
    /// 対象振替日: 02, 09, 12, 22, 26日
    /// 前提条件: PreviousTransferRound = "2"(JOB2済)
    /// JOB3専用処理: KOZFPBKR(銀行結果受信)、KOZ078DS(不能データDS処理)
    /// UEDAGO: 26日のみ IsUedaAfter=true (上田ケーブル後処理)
    /// </summary>
    Round3 = 3
}
