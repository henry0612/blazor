namespace UnifiedAccount.Domain.ValueObjects;

/// <summary>
/// 金融機関コード＋支店コードの複合値 CHAR(4)+CHAR(3)
/// </summary>
public record BankBranchCode
{
    /// <summary>
    /// 銀行コード値を取得または設定する。
    /// </summary>
    public string BankCodeValue { get; }
    /// <summary>
    /// 支店コード値を取得または設定する。
    /// </summary>
    public string BranchCodeValue { get; }

    public BankBranchCode(string bankCode, string branchCode)
    {
        if (string.IsNullOrWhiteSpace(bankCode) || bankCode.Length != 4)
            throw new ArgumentException("金融機関コードは4桁である必要があります。", nameof(bankCode));
        if (string.IsNullOrWhiteSpace(branchCode) || branchCode.Length != 3)
            throw new ArgumentException("支店コードは3桁である必要があります。", nameof(branchCode));

        BankCodeValue = bankCode;
        BranchCodeValue = branchCode;
    }

    public override string ToString() => $"{BankCodeValue}{BranchCodeValue}";
}


