namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// 口座番号 CHAR(7)
/// </summary>
public record AccountNumber
{
    /// <summary>
    /// 値を取得または設定する。
    /// </summary>
    public string Value { get; }

    public AccountNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 7 || !value.All(char.IsDigit))
            throw new ArgumentException("口座番号は数字7桁である必要があります。", nameof(value));
        Value = value;
    }

    public static implicit operator string(AccountNumber num) => num.Value;
    public override string ToString() => Value;
}


