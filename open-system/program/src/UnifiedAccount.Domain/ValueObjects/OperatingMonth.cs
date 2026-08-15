namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// 運用月 NVARCHAR(2)
/// </summary>
public record OperatingMonth
{
    public int Number { get; }

    public string Code => Number.ToString("D2");

    private OperatingMonth(int number)
    {
        Number = number;
    }


    /// <summary>
    /// 値が有効な運用月か判定し、OperatingMonthを生成する。
    /// </summary>
    public static bool TryCreate(
       string? value,
       out OperatingMonth? operatingMonth)
    {
        operatingMonth = null;

        // "1"は不可、"01"のみ許可
        if (value is null || value.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(value, out var number))
        {
            return false;
        }

        if (number is < 1 or > 12)
        {
            return false;
        }

        operatingMonth = new OperatingMonth(number);
        return true;
    }

    public static OperatingMonth Parse(string value)
    {
        if (!TryCreate(value, out var operatingMonth))
        {
            throw new ArgumentException(
                "OperatingMonthは01～12で指定してください。",
                nameof(value));
        }

        return operatingMonth!;
    }

    public override string ToString() => Code;
}
