namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// 金額値オブジェクト (非負のdecimal)
/// </summary>
public record MoneyAmount
{
    /// <summary>
    /// 値を取得または設定する。
    /// </summary>
    public decimal Value { get; }

    public MoneyAmount(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("金額は0以上である必要があります。", nameof(value));
        Value = value;
    }

    /// <summary>
    /// 値が有効な金額か判定し、MoneyAmountを生成する。
    /// </summary>
    public static bool TryCreate(
        decimal? value,
        out MoneyAmount? moneyAmount)
    {
        moneyAmount = null;

        if (value is null || value < 0)
        {
            return false;
        }

        moneyAmount = new MoneyAmount(value.Value);
        return true;
    }

    public static MoneyAmount operator +(MoneyAmount a, MoneyAmount b)
        => new(a.Value + b.Value);

    public static MoneyAmount operator -(MoneyAmount a, MoneyAmount b)
        => new(a.Value - b.Value);

    /// <summary>
    /// 金額を3桁区切りの文字列表現に変換する。
    /// </summary>
    public override string ToString() => Value.ToString("N0");
}




