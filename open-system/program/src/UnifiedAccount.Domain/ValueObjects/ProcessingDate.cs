namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// 処理日 (営業日バリデーション付き)
/// </summary>
public record ProcessingDate
{
    /// <summary>
    /// 値を取得または設定する。
    /// </summary>
    public DateOnly Value { get; }

    public ProcessingDate(DateOnly value)
    {
        if (value == default)
            throw new ArgumentException("処理日が無効です。", nameof(value));
        Value = value;
    }

    public ProcessingDate(int year, int month, int day)
        : this(new DateOnly(year, month, day)) { }

    public static implicit operator DateOnly(ProcessingDate date) => date.Value;
    /// <summary>
    /// 処理日を yyyy-MM-dd 形式の文字列表現に変換する。
    /// </summary>
    public override string ToString() => Value.ToString("yyyy-MM-dd");
}




