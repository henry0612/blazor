namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// 支店コード CHAR(3)
/// </summary>
public record BranchCode
{
    /// <summary>
    /// 値を取得または設定する。
    /// </summary>
    public string Value { get; }

    public BranchCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 3 || !value.All(char.IsDigit))
            throw new ArgumentException("支店コードは数字3桁である必要があります。", nameof(value));
        Value = value;
    }

    public static implicit operator string(BranchCode code) => code.Value;
    public override string ToString() => Value;
}


