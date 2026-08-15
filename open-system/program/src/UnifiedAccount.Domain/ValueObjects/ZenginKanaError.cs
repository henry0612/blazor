namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// 全銀カナ正規化で検出した誤りの種別。
/// </summary>
public enum ZenginKanaErrorCode
{
    /// <summary>
    /// 全銀許可文字以外の文字が含まれている。
    /// </summary>
    InvalidCharacter,

    /// <summary>
    /// 正規化後の文字数が上限を超えている。
    /// </summary>
    ExceedsMaxLength
}

/// <summary>
/// 全銀カナ正規化で検出した誤りを表す。
/// </summary>
/// <param name="Code">誤りの種別。</param>
/// <param name="Message">利用者向けの説明。<see cref="ZenginKanaErrorCode.InvalidCharacter"/> のときは文字位置を含む。</param>
public sealed record ZenginKanaError(ZenginKanaErrorCode Code, string Message);
