using System.Diagnostics.CodeAnalysis;
using UnifiedAccount.Domain.ValueObjects;

namespace UnifiedAccount.Domain.Interfaces;

/// <summary>
/// 文字コード変換 (JCVJEBC相当)
/// </summary>
public interface ICharacterConverter
{
    /// <summary>
    /// 半角文字を全角へ変換する。
    /// </summary>
    /// <param name="input">変換対象。null のときは null を返す。</param>
    [return: NotNullIfNotNull(nameof(input))]
    string? HankakuToZenkaku(string? input);

    /// <summary>
    /// 全角文字を半角へ変換する。
    /// </summary>
    /// <param name="input">変換対象。null のときは null を返す。</param>
    [return: NotNullIfNotNull(nameof(input))]
    string? ZenkakuToHankaku(string? input);

    /// <summary>
    /// 半角カナを全角カナへ正規化する。
    /// </summary>
    /// <param name="input">変換対象。null のときは null を返す。</param>
    [return: NotNullIfNotNull(nameof(input))]
    string? NormalizeKana(string? input);

    /// <summary>
    /// 全銀許可文字へ正規化して検証する。
    /// </summary>
    /// <param name="input">検証対象。null のときは正規化せず有効として返す。</param>
    /// <param name="maxLength">正規化後に許容する最大文字数。</param>
    ZenginKanaResult NormalizeZenginKana(string? input, int maxLength = 15);
}
