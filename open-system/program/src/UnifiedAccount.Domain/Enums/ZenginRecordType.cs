namespace UnifiedAccount.Domain.Enums;

/// <summary>
/// 全銀レコード区分
/// </summary>
public enum ZenginRecordType
{
    /// <summary>
    /// ヘッダ
    /// </summary>
    Header = 1,
    /// <summary>
    /// データ
    /// </summary>
    Data = 2,
    /// <summary>
    /// トレーラ
    /// </summary>
    Trailer = 8,
    /// <summary>
    /// エンド
    /// </summary>
    End = 9
}

