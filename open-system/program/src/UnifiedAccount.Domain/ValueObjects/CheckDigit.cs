namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// チェックデジット CHAR(1)
/// </summary>
public record CheckDigit
{
    /// <summary>
    /// 値を取得または設定する。
    /// </summary>
    public string Value { get; }

    public CheckDigit(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 1)
            throw new ArgumentException("チェックデジットは1桁である必要があります。", nameof(value));
        Value = value;
    }

    public static implicit operator string(CheckDigit cd) => cd.Value;
    public override string ToString() => Value;
}


