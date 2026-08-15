using FluentAssertions;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Domain.Tests.Entities;

public class BankBranchTests
{
    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void BankBranch_ShouldHaveDefaultValues()
    {
        var branch = new BankBranch();
        branch.BankCode.Should().Be(string.Empty);
        branch.BranchCode.Should().Be(string.Empty);
        branch.BankNameKana.Should().Be(string.Empty);
        branch.BranchNameKana.Should().Be(string.Empty);
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void BankBranch_ShouldStoreProperties()
    {
        var branch = new BankBranch
        {
            BankCode = "0143",
            BranchCode = "001",
            BankNameKana = "ﾊﾁｼﾞﾕｳﾆ",
            BranchNameKana = "ﾎﾝﾃﾝ",
            BankNameKanji = "八十二銀行",
            BranchNameKanji = "本店",
            KanjiSetFlag = "1"
        };

        branch.BankCode.Should().Be("0143");
        branch.BranchCode.Should().Be("001");
        branch.BankNameKanji.Should().Be("八十二銀行");
    }
}










