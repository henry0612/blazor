using FluentAssertions;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Domain.Tests.Entities;

public class CompanyTests
{
    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void Company_ShouldHaveDefaultValues()
    {
        var company = new Company();
        company.CompanyCode.Should().Be(string.Empty);
        company.CompanyNameKana.Should().Be(string.Empty);
        company.ConsignorCode.Should().Be(string.Empty);
        company.SuspendFlag.Should().Be("0");
        company.PreviousTransferRound.Should().Be("0");
        company.CurrentTransferRound.Should().Be("0");
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void Company_ShouldStoreProperties()
    {
        var company = new Company
        {
            CompanyCode = "TEST01",
            CompanyNameKana = "テストカイシャ",
            CompanyNameKanji = "テスト会社",
            ConsignorCode = "1234567890",
            BasicFee = 100m,
            AdminFee = 50m
        };

        company.CompanyCode.Should().Be("TEST01");
        company.CompanyNameKana.Should().Be("テストカイシャ");
        company.BasicFee.Should().Be(100m);
    }

    /// <summary>このテストでは 振替回状態を更新したとき、処理が許可される。</summary>
    [Fact]
    public void Company_ShouldAllowTransferRoundStateUpdates()
    {
        var company = new Company
        {
            CompanyCode = "TEST01",
            CompanyNameKana = "テストカイシャ",
            ConsignorCode = "1234567890",
            PreviousTransferRound = "2",
            CurrentTransferRound = "3"
        };

        company.PreviousTransferRound.Should().Be("2");
        company.CurrentTransferRound.Should().Be("3");
    }
}










