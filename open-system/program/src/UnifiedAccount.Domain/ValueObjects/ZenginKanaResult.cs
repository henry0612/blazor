namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// 全銀カナ正規化結果を表す。
/// </summary>
public sealed class ZenginKanaResult
{
    /// <summary>
    /// 正規化結果を生成する。
    /// </summary>
    /// <param name="normalizedValue">正規化後の値。入力が null のときは null。</param>
    /// <param name="isValid">全銀の許可文字と最大長を満たすとき true。</param>
    /// <param name="errors">検出した誤り。誤りがないときは空。</param>
    public ZenginKanaResult(
        string? normalizedValue,
        bool isValid,
        IReadOnlyList<ZenginKanaError>? errors = null)
    {
        NormalizedValue = normalizedValue;
        IsValid = isValid;
        Errors = errors ?? [];
    }

    /// <summary>
    /// 正規化後の値を取得する。入力が null のときは null を返す。
    /// </summary>
    public string? NormalizedValue { get; }

    /// <summary>
    /// 全銀の許可文字と最大長を満たすかどうかを取得する。
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// 検出した誤りを取得する。誤りがないときは空のコレクションを返す。
    /// </summary>
    public IReadOnlyList<ZenginKanaError> Errors { get; }
}
