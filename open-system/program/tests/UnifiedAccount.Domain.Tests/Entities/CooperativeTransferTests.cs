using FluentAssertions;
using UnifiedAccount.Domain.Entities.Cho;

namespace UnifiedAccount.Domain.Tests.Entities;

public class CooperativeTransferTests
{
    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void CooperativeTransfer_ShouldHaveDefaultValues()
    {
        var transfer = new CooperativeTransfer();

        transfer.RecordType.Should().Be(string.Empty);
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void CooperativeTransfer_ShouldStoreHeaderRecord()
    {
        var transfer = new CooperativeTransfer
        {
            RecordType = "1",
            ConsignorCode = "1234567890",
            ConsignorNameKana = "キョウサイレンテスト",
            WithdrawalMonth = "03",
            WithdrawalDay = "27",
            BankCode = "0143",
            BranchCode = "001"
        };

        transfer.RecordType.Should().Be("1");
        transfer.ConsignorCode.Should().Be("1234567890");
        transfer.ConsignorNameKana.Should().Be("キョウサイレンテスト");
        transfer.WithdrawalMonth.Should().Be("03");
        transfer.WithdrawalDay.Should().Be("27");
        transfer.BankCode.Should().Be("0143");
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void CooperativeTransfer_ShouldStoreDetailRecord()
    {
        var transfer = new CooperativeTransfer
        {
            RecordType = "2",
            ContractNo = "0001",
            MemberCode = "M00001",
            PlanCode = "P001",
            CooperativeNo = "001",
            BranchOfficeNo = "010",
            InsuranceType = "01",
            PaymentMethod = "1",
            ContractDate = new DateOnly(2020, 4, 1),
            AnnualMonthlyType = "M",
            AccountType = "1",
            AccountNo = "1234567",
            DepositorName = "ﾀﾅｶ ﾀﾛｳ",
            Amount = 15000m,
            NewCode = "0",
            InvariantNo = "12345678901234"
        };

        transfer.RecordType.Should().Be("2");
        transfer.MemberCode.Should().Be("M00001");
        transfer.Amount.Should().Be(15000m);
        transfer.AccountNo.Should().Be("1234567");
        transfer.ContractDate.Should().Be(new DateOnly(2020, 4, 1));
        transfer.InvariantNo.Should().Be("12345678901234");
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void CooperativeTransfer_ShouldStoreTrailerRecord()
    {
        var transfer = new CooperativeTransfer
        {
            RecordType = "8",
            TotalCount = 50,
            TotalAmount = 750000m,
            SettledCount = 48,
            SettledAmount = 720000m,
            FailedCount = 2,
            FailedAmount = 30000m
        };

        transfer.RecordType.Should().Be("8");
        transfer.TotalCount.Should().Be(50);
        transfer.TotalAmount.Should().Be(750000m);
        transfer.SettledCount.Should().Be(48);
        transfer.SettledAmount.Should().Be(720000m);
        transfer.FailedCount.Should().Be(2);
        transfer.FailedAmount.Should().Be(30000m);
    }

    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void CooperativeTransfer_OptionalFields_ShouldBeNullByDefault()
    {
        var transfer = new CooperativeTransfer();

        transfer.ConsignorCode.Should().BeNull();
        transfer.ConsignorNameKana.Should().BeNull();
        transfer.BankCode.Should().BeNull();
        transfer.BranchCode.Should().BeNull();
        transfer.MemberCode.Should().BeNull();
        transfer.Amount.Should().BeNull();
        transfer.ResultCode.Should().BeNull();
        transfer.KozConsignorCode.Should().BeNull();
        transfer.MatchYearMonth.Should().BeNull();
        transfer.MatchSeqNo.Should().BeNull();
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void CooperativeTransfer_PostalFields_ShouldStoreValues()
    {
        var transfer = new CooperativeTransfer
        {
            PostalBankCode = "9900",
            PostalBranchCode = "001",
            PostalAccountType = "1",
            PostalAccountNo = "9876543210"
        };

        transfer.PostalBankCode.Should().Be("9900");
        transfer.PostalBranchCode.Should().Be("001");
        transfer.PostalAccountType.Should().Be("1");
        transfer.PostalAccountNo.Should().Be("9876543210");
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void CooperativeTransfer_MatchingFields_ShouldStoreValues()
    {
        var transfer = new CooperativeTransfer
        {
            KozConsignorCode = "KOZ001",
            MatchYearMonth = "202603",
            MatchSeqNo = 1
        };

        transfer.KozConsignorCode.Should().Be("KOZ001");
        transfer.MatchYearMonth.Should().Be("202603");
        transfer.MatchSeqNo.Should().Be(1);
    }
}










