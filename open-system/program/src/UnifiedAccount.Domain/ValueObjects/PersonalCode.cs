namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// 個人コード CHAR(12)
/// </summary>
public record PersonalCode
{
    /// <summary>
    /// 値を取得または設定する。
    /// </summary>
    public string Value { get; }

    public PersonalCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 12)
            throw new ArgumentException("個人コードは12桁である必要があります。", nameof(value));
        Value = value;
    }

    public static implicit operator string(PersonalCode code) => code.Value;
    public override string ToString() => Value;
}


