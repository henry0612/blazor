namespace UnifiedAccount.Domain.Services;

/// <summary>
/// チェックデジット計算共通ロジック。
/// 入力契約は 222 F-INF-006 チェックデジット検証 共通プログラム定義書 §3.2、例外仕様は同 §4.5 に従う。
/// </summary>
public static class CheckDigitCalculator
{
    /// <summary>
    /// accountNumber の最大桁数。222 §3.2 メソッド別入力表による。
    /// 契約者コード（会社コード6桁＋個人コード12桁）の18桁が現行の最大長である。
    /// 現行 COBOL の KOZ035N / KOZ830 は D-KEIYAKU OCCURS 18 の固定長で算出する。
    /// </summary>
    private const int MaxAccountNumberLength = 18;

    /// <summary>
    /// Mod10でチェックデジットを計算する。
    /// </summary>
    /// <param name="accountNumber">計算対象のASCII半角数字文字列（1〜18桁）。</param>
    /// <exception cref="ArgumentException">
    /// null、空、1〜18桁の範囲外、またはASCII半角数字以外を含むとき。
    /// </exception>
    public static string CalculateMod10(string accountNumber)
    {
        ValidateAccountNumber(accountNumber);

        int sum = 0;
        bool doubleFlag = true;
        for (int i = accountNumber.Length - 1; i >= 0; i--)
        {
            int digit = accountNumber[i] - '0';
            if (doubleFlag)
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            sum += digit;
            doubleFlag = !doubleFlag;
        }

        int remainder = sum % 10;
        return remainder == 0 ? "0" : (10 - remainder).ToString();
    }

    /// <summary>
    /// Mod11でチェックデジットを計算する。
    /// </summary>
    /// <param name="accountNumber">計算対象のASCII半角数字文字列（1〜18桁）。</param>
    /// <exception cref="ArgumentException">
    /// null、空、1〜18桁の範囲外、またはASCII半角数字以外を含むとき。
    /// </exception>
    public static string CalculateMod11(string accountNumber)
    {
        ValidateAccountNumber(accountNumber);

        int sum = 0;
        int weight = 2;
        for (int i = accountNumber.Length - 1; i >= 0; i--)
        {
            sum += (accountNumber[i] - '0') * weight;
            weight = weight >= 7 ? 2 : weight + 1;
        }

        int remainder = 11 - (sum % 11);
        return remainder switch
        {
            10 => "0",
            11 => "0",
            _ => remainder.ToString()
        };
    }

    /// <summary>
    /// 会社コードと個人コードを連結し、抽出したASCII半角数字へMod10を適用する。
    /// </summary>
    /// <param name="companyCode">会社コード（1文字以上）。</param>
    /// <param name="personalCode">個人コード（1文字以上）。</param>
    /// <exception cref="ArgumentException">
    /// いずれかがnullまたは空のとき、または連結値にASCII半角数字が存在しないとき。
    /// </exception>
    public static string CalculateForCompanyAndPersonal(string companyCode, string personalCode)
    {
        if (string.IsNullOrEmpty(companyCode))
        {
            throw new ArgumentException("会社コードが空です。", nameof(companyCode));
        }

        if (string.IsNullOrEmpty(personalCode))
        {
            throw new ArgumentException("個人コードが空です。", nameof(personalCode));
        }

        var combined = $"{companyCode}{personalCode}";

        // 222 §3.1: 計算対象はASCII半角数字だけとする。
        // char.IsDigit は Unicode カテゴリ Nd を真とし全角数字も抽出するため使用しない（同 §7.1 差異No.2）。
        var digits = new string(combined.Where(char.IsAsciiDigit).ToArray());

        // 222 §4.3 手順3: 抽出結果が空の場合は例外とする。
        // 連結値の長さを代用する旧実装のフォールバックは同 §7. で明示的に否定されている（同 §7.1 差異No.3）。
        if (string.IsNullOrEmpty(digits))
        {
            throw new ArgumentException(
                "会社コードと個人コードの連結値にASCII半角数字が含まれていません。",
                nameof(companyCode));
        }

        return CalculateMod10(digits);
    }

    /// <summary>
    /// 222 §3.2 の accountNumber 入力表に従って検証する。
    /// </summary>
    private static void ValidateAccountNumber(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber))
        {
            throw new ArgumentException("口座番号が空です。", nameof(accountNumber));
        }

        if (accountNumber.Length > MaxAccountNumberLength)
        {
            throw new ArgumentException(
                $"口座番号は{MaxAccountNumberLength}桁以内で指定してください。",
                nameof(accountNumber));
        }

        foreach (var character in accountNumber)
        {
            if (!char.IsAsciiDigit(character))
            {
                throw new ArgumentException(
                    "口座番号はASCII半角数字だけで構成してください。",
                    nameof(accountNumber));
            }
        }
    }
}
