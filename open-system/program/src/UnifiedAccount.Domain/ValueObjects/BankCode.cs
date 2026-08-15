namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// 金融機関コード CHAR(4)
/// </summary>
public record BankCode
{
    /// <summary>
    /// 値を取得または設定する。
    /// </summary>
    public string Value { get; }

    public BankCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 4 || !value.All(char.IsDigit))
            throw new ArgumentException("金融機関コードは数字4桁である必要があります。", nameof(value));
        Value = value;
    }

    public static implicit operator string(BankCode code) => code.Value;
    public override string ToString() => Value;
}


