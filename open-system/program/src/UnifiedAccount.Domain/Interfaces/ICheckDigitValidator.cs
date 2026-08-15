namespace UnifiedAccount.Domain.Interfaces;

/// <summary>
/// チェックデジット検証 (KOZAPKT相当)
/// 入力契約は 222 F-INF-006 チェックデジット検証 共通プログラム定義書 §3.2、例外仕様は同 §4.5 に従う。
/// </summary>
public interface ICheckDigitValidator
{
    /// <summary>
    /// Mod10チェックデジットで検証する。
    /// </summary>
    /// <param name="accountNumber">検証対象のASCII半角数字文字列（1〜18桁）。</param>
    /// <param name="checkDigit">照合するチェックデジット（ASCII半角数字1桁）。</param>
    /// <returns>一致するとき true。</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="accountNumber"/> がnull、空、1〜18桁の範囲外、またはASCII半角数字以外を含むとき。
    /// または <paramref name="checkDigit"/> がnull、空、またはASCII半角数字1桁でないとき。
    /// </exception>
    bool ValidateMod10(string accountNumber, string checkDigit);

    /// <summary>
    /// Mod11チェックデジットで検証する。
    /// </summary>
    /// <param name="accountNumber">検証対象のASCII半角数字文字列（1〜18桁）。</param>
    /// <param name="checkDigit">照合するチェックデジット（ASCII半角数字1桁）。</param>
    /// <returns>一致するとき true。</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="accountNumber"/> がnull、空、1〜18桁の範囲外、またはASCII半角数字以外を含むとき。
    /// または <paramref name="checkDigit"/> がnull、空、またはASCII半角数字1桁でないとき。
    /// </exception>
    bool ValidateMod11(string accountNumber, string checkDigit);

    /// <summary>
    /// Mod10でチェックデジットを計算する。
    /// </summary>
    /// <param name="accountNumber">計算対象のASCII半角数字文字列（1〜18桁）。</param>
    /// <returns>ASCII半角数字1桁。</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="accountNumber"/> がnull、空、1〜18桁の範囲外、またはASCII半角数字以外を含むとき。
    /// </exception>
    string CalculateMod10(string accountNumber);

    /// <summary>
    /// Mod11でチェックデジットを計算する。
    /// </summary>
    /// <param name="accountNumber">計算対象のASCII半角数字文字列（1〜18桁）。</param>
    /// <returns>ASCII半角数字1桁。</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="accountNumber"/> がnull、空、1〜18桁の範囲外、またはASCII半角数字以外を含むとき。
    /// </exception>
    string CalculateMod11(string accountNumber);

    /// <summary>
    /// 会社コードと個人コードを連結し、抽出したASCII半角数字へMod10を適用する。
    /// </summary>
    /// <param name="companyCode">会社コード（1文字以上）。</param>
    /// <param name="personalCode">個人コード（1文字以上）。</param>
    /// <returns>ASCII半角数字1桁。</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="companyCode"/> または <paramref name="personalCode"/> がnullまたは空のとき、
    /// あるいは連結値にASCII半角数字が存在しないとき。
    /// </exception>
    string Calculate(string companyCode, string personalCode);

    /// <summary>
    /// <see cref="Calculate"/> の結果でチェックデジットを検証する。
    /// </summary>
    /// <param name="companyCode">会社コード（1文字以上）。</param>
    /// <param name="personalCode">個人コード（1文字以上）。</param>
    /// <param name="checkDigit">照合するチェックデジット（ASCII半角数字1桁）。</param>
    /// <returns>一致するとき true。</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="companyCode"/> または <paramref name="personalCode"/> がnullまたは空のとき、
    /// 連結値にASCII半角数字が存在しないとき、
    /// または <paramref name="checkDigit"/> がnull、空、またはASCII半角数字1桁でないとき。
    /// </exception>
    bool Validate(string companyCode, string personalCode, string checkDigit);
}

