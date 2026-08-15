using UnifiedAccount.Domain.Interfaces;
using UnifiedAccount.Domain.Services;

namespace UnifiedAccount.Infrastructure.Services;

/// <summary>
/// チェックデジット検証。
/// 個人コードの C/D は現行 COBOL の KOZ035N / KOZ830 の CHECK-DIGIT セクション相当であり、
/// Calculate(companyCode, personalCode) がその互換実装である。
/// KOZAPKT / KOZACK は口座番号の C/D（金融機関別の重みテーブル方式）であり、本クラスの対象ではない。
/// 入力契約は 222 F-INF-006 チェックデジット検証 共通プログラム定義書 §3.2、例外仕様は同 §4.5 に従う。
/// </summary>
public class CheckDigitValidator : ICheckDigitValidator
{
    /// <inheritdoc />
    public bool ValidateMod10(string accountNumber, string checkDigit)
    {
        // 222 §4.4 手順1: 引数を先に検証する。accountNumber の検証は CalculateMod10 が行う。
        var expected = CalculateMod10(accountNumber);
        ValidateCheckDigit(checkDigit);

        // 222 §3.3: 正規化せず Ordinal 比較する。
        return string.Equals(expected, checkDigit, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public bool ValidateMod11(string accountNumber, string checkDigit)
    {
        var expected = CalculateMod11(accountNumber);
        ValidateCheckDigit(checkDigit);

        return string.Equals(expected, checkDigit, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public string CalculateMod10(string accountNumber)
    {
        return CheckDigitCalculator.CalculateMod10(accountNumber);
    }

    /// <inheritdoc />
    public string CalculateMod11(string accountNumber)
    {
        return CheckDigitCalculator.CalculateMod11(accountNumber);
    }

    /// <inheritdoc />
    public string Calculate(string companyCode, string personalCode)
    {
        return CheckDigitCalculator.CalculateForCompanyAndPersonal(companyCode, personalCode);
    }

    /// <inheritdoc />
    public bool Validate(string companyCode, string personalCode, string checkDigit)
    {
        var expected = Calculate(companyCode, personalCode);
        ValidateCheckDigit(checkDigit);

        return string.Equals(expected, checkDigit, StringComparison.Ordinal);
    }

    /// <summary>
    /// 222 §3.2 の checkDigit 入力表に従って検証する。
    /// </summary>
    private static void ValidateCheckDigit(string checkDigit)
    {
        if (string.IsNullOrEmpty(checkDigit)
            || checkDigit.Length != 1
            || !char.IsAsciiDigit(checkDigit[0]))
        {
            throw new ArgumentException(
                "チェックデジットはASCII半角数字1桁で指定してください。",
                nameof(checkDigit));
        }
    }
}

