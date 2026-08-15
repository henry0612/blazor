namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// 周期 NVARCHAR(2)
/// </summary>
public record Cycle
{
    public int Number { get; }

    public string Code => Number.ToString("D2");

    private Cycle(int number)
    {
        Number = number;
    }


    /// <summary>
    /// 値が有効な周期か判定し、Cycleを生成する。
    /// </summary>
    public static bool TryCreate(
       string? value,
       out Cycle? cycle)
    {
        cycle = null;

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

        cycle = new Cycle(number);
        return true;
    }

    public static Cycle Parse(string value)
    {
        if (!TryCreate(value, out var cycle))
        {
            throw new ArgumentException(
                "Cycleは01～12で指定してください。",
                nameof(value));
        }

        return cycle!;
    }

    public override string ToString() => Code;
}
