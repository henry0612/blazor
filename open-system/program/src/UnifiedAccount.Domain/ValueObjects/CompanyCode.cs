namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// 会社コード CHAR(6)
/// </summary>
public record CompanyCode
{
    /// <summary>
    /// 値を取得または設定する。
    /// </summary>
    public string Value { get; }

    public CompanyCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 6)
            throw new ArgumentException("会社コードは6桁である必要があります。", nameof(value));
        Value = value;
    }

    public static implicit operator string(CompanyCode code) => code.Value;
    public override string ToString() => Value;
}


