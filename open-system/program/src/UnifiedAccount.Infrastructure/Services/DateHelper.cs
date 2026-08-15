using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Infrastructure.Services;

/// <summary>
/// 日付操作ヘルパー (DATEPKT 相当)
/// 和暦情報はJapaneseErasテーブルから取得し、キャッシュして使用する。
/// 新元号への対応はDBレコード追加のみで完了する。
/// </summary>
public class DateHelper
{
    /// <summary>
    /// 元号一覧を保持する。
    /// </summary>
    private readonly List<JapaneseEra> _eras;

    /// <summary>
    /// コンストラクタ: JapaneseErasテーブルのデータを受け取りキャッシュする。
    /// DI登録時にDbContextから取得して渡す。
    /// </summary>
    public DateHelper(IEnumerable<JapaneseEra> eras)
    {
        _eras = eras.OrderByDescending(e => e.StartDate).ToList();
        if (_eras.Count == 0)
            throw new InvalidOperationException("JapaneseErasテーブルにデータがありません。シードデータを確認してください。");
    }

    /// <summary>
    /// 西暦→和暦変換 (JapaneseErasテーブルから判定)
    /// </summary>
    public (string Era, int Year) ToWareki(DateOnly date)
    {
        var era = _eras.FirstOrDefault(e => date >= e.StartDate && (e.EndDate == null || date <= e.EndDate))
            ?? throw new ArgumentException($"該当する元号が見つかりません: {date}");
        return (era.Name, date.Year - era.BaseYear);
    }

    /// <summary>
    /// 和暦→西暦変換 (名称またはアルファベット略称で検索)
    /// </summary>
    /// <param name="era">元号を指定する。</param>
    /// <param name="year">年を指定する。</param>
    /// <param name="month">月を指定する。</param>
    /// <param name="day">日を指定する。</param>
    public DateOnly FromWareki(string era, int year, int month, int day)
    {
        var matched = _eras.FirstOrDefault(e => e.Name == era || e.Abbreviation == era)
            ?? throw new ArgumentException($"不明な元号: {era}");
        return new DateOnly(matched.BaseYear + year, month, day);
    }

    /// <summary>
    /// 月末日取得
    /// </summary>
    /// <param name="year">年を指定する。</param>
    /// <param name="month">月を指定する。</param>
    public DateOnly GetLastDayOfMonth(int year, int month)
    {
        return new DateOnly(year, month, DateTime.DaysInMonth(year, month));
    }

    /// <summary>
    /// YYYYMMDD文字列→DateOnly (COBOL日付フォーマット変換)
    /// </summary>
    /// <param name="yyyymmdd">日付文字列を指定する。</param>
    public DateOnly? ParseCobolDate(string yyyymmdd)
    {
        if (string.IsNullOrWhiteSpace(yyyymmdd) || yyyymmdd == "00000000" || yyyymmdd.Length != 8)
            return null;

        if (int.TryParse(yyyymmdd[..4], out int year) &&
            int.TryParse(yyyymmdd[4..6], out int month) &&
            int.TryParse(yyyymmdd[6..8], out int day))
        {
            try
            {
                return new DateOnly(year, month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                // 222 §5: 実在しない日付は例外ではなく null を返す。
                // DateOnly のコンストラクタが送出するのは ArgumentOutOfRangeException だけであり、
                // 型を指定しない catch は想定外の例外まで隠すため使用しない。
                return null;
            }
        }
        return null;
    }

    /// <summary>
    /// DateOnly→YYYYMMDD文字列
    /// </summary>
    /// <param name="date">日付を指定する。</param>
    public string ToCobolDate(DateOnly? date)
    {
        return date?.ToString("yyyyMMdd") ?? "00000000";
    }

    /// <summary>
    /// 2桁年→4桁年変換 (ウィンドウ方式, CHO向け)
    /// </summary>
    /// <param name="twoDigitYear">2桁年を指定する。</param>
    /// <param name="windowBase">ウィンドウ基準を指定する。</param>
    public int ConvertTwoDigitYear(int twoDigitYear, int windowBase = 50)
    {
        return twoDigitYear >= windowBase ? 1900 + twoDigitYear : 2000 + twoDigitYear;
    }

    /// <summary>
    /// YYYYMMDD文字列→DateOnly変換 (例外あり版)
    /// </summary>
    /// <param name="yyyymmdd">日付文字列を指定する。</param>
    public DateOnly GetProcessDate(string yyyymmdd)
    {
        if (string.IsNullOrWhiteSpace(yyyymmdd) || yyyymmdd.Length != 8)
            throw new ArgumentException($"日付文字列が不正です: '{yyyymmdd}'", nameof(yyyymmdd));

        return ParseCobolDate(yyyymmdd)
            ?? throw new FormatException($"日付として解析できません: '{yyyymmdd}'");
    }

    /// <summary>
    /// DateOnly→YYYYMMDD文字列 (非null版)
    /// </summary>
    /// <param name="date">日付を指定する。</param>
    public string FormatDate(DateOnly date)
    {
        return date.ToString("yyyyMMdd");
    }

    /// <summary>
    /// 和暦6桁文字列 (YYMMDD) → DateOnly変換
    /// 全銀・共済連・FTPファイル読込時に使用。
    /// 元号コードが不明な場合はGuessGengoで推定。
    /// </summary>
    /// <param name="wareki6">和暦6桁を指定する。</param>
    /// <param name="gengoCode">元号コードコードを指定する。</param>
    public DateOnly FromWareki6(string wareki6, int? gengoCode = null)
    {
        if (string.IsNullOrEmpty(wareki6) || wareki6.Length < 6)
            throw new ArgumentException($"和暦6桁として不正です: '{wareki6}'", nameof(wareki6));

        if (!int.TryParse(wareki6[..2], out int yy) ||
            !int.TryParse(wareki6[2..4], out int mm) ||
            !int.TryParse(wareki6[4..6], out int dd))
            throw new FormatException($"和暦6桁の数値解析に失敗: '{wareki6}'");

        int code = gengoCode ?? GuessGengo(yy);
        var era = _eras.FirstOrDefault(e => e.Code == code)
            ?? throw new ArgumentException($"不明な元号コード: {code}");

        return new DateOnly(era.BaseYear + yy, mm, dd);
    }

    /// <summary>
    /// 和暦7桁文字列 (GYYMMDD) → DateOnly変換
    /// G=元号1桁 + YY=年2桁 + MM=月2桁 + DD=日2桁
    /// </summary>
    /// <param name="wareki7">和暦7桁を指定する。</param>
    public DateOnly FromWareki7(string wareki7)
    {
        if (string.IsNullOrEmpty(wareki7) || wareki7.Length < 7)
            throw new ArgumentException($"和暦7桁として不正です: '{wareki7}'", nameof(wareki7));

        if (!int.TryParse(wareki7[..1], out int gengo))
            throw new FormatException($"元号コードの解析に失敗: '{wareki7}'");

        return FromWareki6(wareki7[1..7], gengo);
    }

    /// <summary>
    /// DateOnly→和暦6桁文字列 (YYMMDD)
    /// 全銀・共済連ファイル書込時に使用。
    /// </summary>
    /// <param name="date">日付を指定する。</param>
    public string ToWareki6(DateOnly date)
    {
        var (_, year) = ToWareki(date);
        return $"{year:D2}{date.Month:D2}{date.Day:D2}";
    }

    /// <summary>
    /// DateOnly→和暦7桁文字列 (GYYMMDD)
    /// G=元号コード (5=令和, 4=平成, 3=昭和)
    /// </summary>
    /// <param name="date">日付を指定する。</param>
    public string ToWareki7(DateOnly date)
    {
        var era = _eras.FirstOrDefault(e => date >= e.StartDate && (e.EndDate == null || date <= e.EndDate))
            ?? throw new ArgumentException($"該当する元号が見つかりません: {date}");
        int year = date.Year - era.BaseYear;
        return $"{era.Code}{year:D2}{date.Month:D2}{date.Day:D2}";
    }

    /// <summary>
    /// 元号推定 (年数から現在有効な元号を推定)
    /// </summary>
    private int GuessGengo(int yy)
    {
        // EndDate==null の元号が現在 → その元号コードを使う
        var current = _eras.FirstOrDefault(e => e.EndDate == null);
        if (current != null && yy <= 20) return current.Code;

        // 年数が大きければ前の元号を順に探す
        foreach (var era in _eras.Where(e => e.EndDate != null).OrderByDescending(e => e.Code))
        {
            if (yy <= (era.EndDate!.Value.Year - era.BaseYear)) return era.Code;
        }
        return _eras.Last().Code; // 最古の元号
    }

    /// <summary>
    /// 全元号一覧を取得 (管理画面用)
    /// </summary>
    public IReadOnlyList<JapaneseEra> GetAllEras() => _eras.AsReadOnly();

    /// <summary>
    /// 元号コードから元号情報を取得
    /// </summary>
    /// <param name="code">コードを指定する。</param>
    public JapaneseEra? GetEraByCode(int code) => _eras.FirstOrDefault(e => e.Code == code);
}




