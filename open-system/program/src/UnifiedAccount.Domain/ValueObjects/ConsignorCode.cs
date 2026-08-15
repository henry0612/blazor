namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// 委託者コード CHAR(10)
/// </summary>
public record ConsignorCode
{
    /// <summary>
    /// 値を取得または設定する。
    /// </summary>
    public string Value { get; }

    public ConsignorCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 10)
            throw new ArgumentException("委託者コードは10桁である必要があります。", nameof(value));
        Value = value;
    }

    public static implicit operator string(ConsignorCode code) => code.Value;
    public override string ToString() => Value;
}


