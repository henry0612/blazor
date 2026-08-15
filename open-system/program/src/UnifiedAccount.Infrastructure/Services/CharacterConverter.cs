using System.Diagnostics.CodeAnalysis;
using System.Text;
using UnifiedAccount.Domain.Interfaces;
using UnifiedAccount.Domain.ValueObjects;

namespace UnifiedAccount.Infrastructure.Services;

/// <summary>
/// 文字コード変換サービス (JCVJEBC 相当)
/// </summary>
public class CharacterConverter : ICharacterConverter
{
    private const string ZenginAllowedCharacters =
        "ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜｦﾝﾞﾟ()-.0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ ";

    /// <summary>
    /// 半角→全角変換
    /// </summary>
    /// <param name="input">入力を指定する。</param>
    [return: NotNullIfNotNull(nameof(input))]
    public string? HankakuToZenkaku(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var result = new char[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c >= '!' && c <= '~')
            {
                // ASCII印字可能文字 → 全角
                result[i] = (char)(c + 0xFEE0);
            }
            else if (c == ' ')
            {
                result[i] = '\u3000'; // 全角スペース
            }
            else
            {
                result[i] = c;
            }
        }
        return new string(result);
    }

    /// <summary>
    /// 全角→半角変換
    /// </summary>
    /// <param name="input">入力を指定する。</param>
    [return: NotNullIfNotNull(nameof(input))]
    public string? ZenkakuToHankaku(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var result = new char[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c >= '\uFF01' && c <= '\uFF5E')
            {
                // 全角ASCII → 半角
                result[i] = (char)(c - 0xFEE0);
            }
            else if (c == '\u3000')
            {
                result[i] = ' '; // 半角スペース
            }
            else
            {
                result[i] = c;
            }
        }
        return new string(result);
    }

    /// <summary>
    /// カナ正規化 (半角カナ→全角カナ)
    /// </summary>
    /// <param name="input">入力を指定する。</param>
    [return: NotNullIfNotNull(nameof(input))]
    public string? NormalizeKana(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // 半角カタカナ→全角カタカナの変換マップ
        var sb = new StringBuilder(input.Length);
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c >= '\uFF65' && c <= '\uFF9F')
            {
                // 半角カタカナ範囲
                sb.Append(HankakuKanaToZenkaku(c, i + 1 < input.Length ? input[i + 1] : '\0', out bool consumed));
                if (consumed)
                {
                    i++;
                }
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 全銀許可文字へ正規化して検証する。
    /// </summary>
    /// <param name="input">検証対象。null のときは正規化せず有効として返す。</param>
    /// <param name="maxLength">正規化後に許容する最大文字数。</param>
    public ZenginKanaResult NormalizeZenginKana(string? input, int maxLength = 15)
    {
        if (input is null)
        {
            return new ZenginKanaResult(null, true);
        }

        var normalized = new StringBuilder(input.Length);
        foreach (var character in input)
        {
            if (TryNormalizeZenginCharacter(character, out var replacement))
            {
                normalized.Append(replacement);
            }
            else
            {
                normalized.Append(character);
            }
        }

        var errors = new List<ZenginKanaError>();

        // 222 §2: 許可外文字は種別ごとに1件へ集約し、位置はメッセージへ含める。
        var invalidPositions = new List<int>();
        for (var index = 0; index < normalized.Length; index++)
        {
            if (!ZenginAllowedCharacters.Contains(normalized[index]))
            {
                invalidPositions.Add(index + 1);
            }
        }

        if (invalidPositions.Count > 0)
        {
            errors.Add(new ZenginKanaError(
                ZenginKanaErrorCode.InvalidCharacter,
                $"全銀許可文字以外の文字が含まれています（位置: {string.Join("、", invalidPositions)}）。"));
        }

        if (normalized.Length > maxLength)
        {
            errors.Add(new ZenginKanaError(
                ZenginKanaErrorCode.ExceedsMaxLength,
                $"正規化後の文字数は{maxLength}文字以内である必要があります。"));
        }

        return new ZenginKanaResult(normalized.ToString(), errors.Count == 0, errors);
    }

    private static bool TryNormalizeZenginCharacter(char character, out string replacement)
    {
        replacement = character switch
        {
            'ｯ' => "ﾂ",
            'ｬ' => "ﾔ",
            'ｭ' => "ﾕ",
            'ｮ' => "ﾖ",
            'ｧ' => "ｱ",
            'ｨ' => "ｲ",
            'ｩ' => "ｳ",
            'ｪ' => "ｴ",
            'ｫ' => "ｵ",
            'ｰ' => "-",
            'ガ' => "ｶﾞ",
            'ギ' => "ｷﾞ",
            'グ' => "ｸﾞ",
            'ゲ' => "ｹﾞ",
            'ゴ' => "ｺﾞ",
            'ザ' => "ｻﾞ",
            'ジ' => "ｼﾞ",
            'ズ' => "ｽﾞ",
            'ゼ' => "ｾﾞ",
            'ゾ' => "ｿﾞ",
            'ダ' => "ﾀﾞ",
            'ヂ' => "ﾁﾞ",
            'ヅ' => "ﾂﾞ",
            'デ' => "ﾃﾞ",
            'ド' => "ﾄﾞ",
            'バ' => "ﾊﾞ",
            'ビ' => "ﾋﾞ",
            'ブ' => "ﾌﾞ",
            'ベ' => "ﾍﾞ",
            'ボ' => "ﾎﾞ",
            'パ' => "ﾊﾟ",
            'ピ' => "ﾋﾟ",
            'プ' => "ﾌﾟ",
            'ペ' => "ﾍﾟ",
            'ポ' => "ﾎﾟ",
            'ヴ' => "ｳﾞ",
            _ => string.Empty
        };

        return replacement.Length > 0;
    }

    private static char HankakuKanaToZenkaku(char c, char next, out bool consumed)
    {
        consumed = false;
        // 濁点・半濁点の合成処理
        if (next == '\uFF9E') // 濁点
        {
            var dakuten = GetDakutenChar(c);
            if (dakuten != '\0')
            {
                consumed = true;
                return dakuten;
            }
        }
        if (next == '\uFF9F') // 半濁点
        {
            var handakuten = GetHandakutenChar(c);
            if (handakuten != '\0')
            {
                consumed = true;
                return handakuten;
            }
        }

        // 単純変換テーブル (FF65-FF9F → 30A1-30F3範囲)
        return c switch
        {
            '\uFF66' => '\u30F2', // ヲ
            '\uFF67' => '\u30A1', // ァ
            '\uFF68' => '\u30A3', // ィ
            '\uFF69' => '\u30A5', // ゥ
            '\uFF6A' => '\u30A7', // ェ
            '\uFF6B' => '\u30A9', // ォ
            '\uFF6C' => '\u30E3', // ャ
            '\uFF6D' => '\u30E5', // ュ
            '\uFF6E' => '\u30E7', // ョ
            '\uFF6F' => '\u30C3', // ッ
            '\uFF70' => '\u30FC', // ー
            '\uFF71' => '\u30A2', // ア
            '\uFF72' => '\u30A4', // イ
            '\uFF73' => '\u30A6', // ウ
            '\uFF74' => '\u30A8', // エ
            '\uFF75' => '\u30AA', // オ
            '\uFF76' => '\u30AB', // カ
            '\uFF77' => '\u30AD', // キ
            '\uFF78' => '\u30AF', // ク
            '\uFF79' => '\u30B1', // ケ
            '\uFF7A' => '\u30B3', // コ
            '\uFF7B' => '\u30B5', // サ
            '\uFF7C' => '\u30B7', // シ
            '\uFF7D' => '\u30B9', // ス
            '\uFF7E' => '\u30BB', // セ
            '\uFF7F' => '\u30BD', // ソ
            '\uFF80' => '\u30BF', // タ
            '\uFF81' => '\u30C1', // チ
            '\uFF82' => '\u30C4', // ツ
            '\uFF83' => '\u30C6', // テ
            '\uFF84' => '\u30C8', // ト
            '\uFF85' => '\u30CA', // ナ
            '\uFF86' => '\u30CB', // ニ
            '\uFF87' => '\u30CC', // ヌ
            '\uFF88' => '\u30CD', // ネ
            '\uFF89' => '\u30CE', // ノ
            '\uFF8A' => '\u30CF', // ハ
            '\uFF8B' => '\u30D2', // ヒ
            '\uFF8C' => '\u30D5', // フ
            '\uFF8D' => '\u30D8', // ヘ
            '\uFF8E' => '\u30DB', // ホ
            '\uFF8F' => '\u30DE', // マ
            '\uFF90' => '\u30DF', // ミ
            '\uFF91' => '\u30E0', // ム
            '\uFF92' => '\u30E1', // メ
            '\uFF93' => '\u30E2', // モ
            '\uFF94' => '\u30E4', // ヤ
            '\uFF95' => '\u30E6', // ユ
            '\uFF96' => '\u30E8', // ヨ
            '\uFF97' => '\u30E9', // ラ
            '\uFF98' => '\u30EA', // リ
            '\uFF99' => '\u30EB', // ル
            '\uFF9A' => '\u30EC', // レ
            '\uFF9B' => '\u30ED', // ロ
            '\uFF9C' => '\u30EF', // ワ
            '\uFF9D' => '\u30F3', // ン
            '\uFF65' => '\u30FB', // ・
            _ => c
        };
    }

    private static char GetDakutenChar(char c) => c switch
    {
        '\uFF76' => '\u30AC', // ガ
        '\uFF77' => '\u30AE', // ギ
        '\uFF78' => '\u30B0', // グ
        '\uFF79' => '\u30B2', // ゲ
        '\uFF7A' => '\u30B4', // ゴ
        '\uFF7B' => '\u30B6', // ザ
        '\uFF7C' => '\u30B8', // ジ
        '\uFF7D' => '\u30BA', // ズ
        '\uFF7E' => '\u30BC', // ゼ
        '\uFF7F' => '\u30BE', // ゾ
        '\uFF80' => '\u30C0', // ダ
        '\uFF81' => '\u30C2', // ヂ
        '\uFF82' => '\u30C5', // ヅ
        '\uFF83' => '\u30C7', // デ
        '\uFF84' => '\u30C9', // ド
        '\uFF8A' => '\u30D0', // バ
        '\uFF8B' => '\u30D3', // ビ
        '\uFF8C' => '\u30D6', // ブ
        '\uFF8D' => '\u30D9', // ベ
        '\uFF8E' => '\u30DC', // ボ
        '\uFF73' => '\u30F4', // ヴ
        _ => '\0'
    };

    private static char GetHandakutenChar(char c) => c switch
    {
        '\uFF8A' => '\u30D1', // パ
        '\uFF8B' => '\u30D4', // ピ
        '\uFF8C' => '\u30D7', // プ
        '\uFF8D' => '\u30DA', // ペ
        '\uFF8E' => '\u30DD', // ポ
        _ => '\0'
    };
}



