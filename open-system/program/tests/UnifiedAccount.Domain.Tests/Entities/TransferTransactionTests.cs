using FluentAssertions;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Domain.Tests.Entities;

public class TransferTransactionTests
{
    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void TransferTransaction_ShouldHaveDefaultValues()
    {
        var tx = new TransferTransaction();

        tx.ConsignorCode.Should().Be(string.Empty);
        tx.BankCode.Should().Be(string.Empty);
        tx.BranchCode.Should().Be(string.Empty);
        tx.AccountType.Should().Be(string.Empty);
        tx.AccountNo.Should().Be(string.Empty);
        tx.DepositorName.Should().Be(string.Empty);
        tx.NewCode.Should().Be("0");
        tx.CompanyCode.Should().Be(string.Empty);
        tx.PersonalCode.Should().Be(string.Empty);
        tx.CheckDigit.Should().Be(string.Empty);
        tx.Amount.Should().Be(0m);
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void TransferTransaction_ShouldStoreProperties()
    {
        var tx = new TransferTransaction
        {
            ConsignorCode = "1234567890",
            ConsignorName = "テスト委託者",
            WithdrawalDate = new DateOnly(2026, 3, 27),
            BankCode = "0143",
            BranchCode = "001",
            BankName = "八十二銀行",
            BranchName = "本店",
            AccountType = "1",
            AccountNo = "1234567890",
            DepositorName = "ﾀﾅｶ ﾀﾛｳ",
            Amount = 50000m,
            CompanyCode = "TEST01",
            PersonalCode = "123456789012",
            CheckDigit = "5"
        };

        tx.ConsignorCode.Should().Be("1234567890");
        tx.WithdrawalDate.Should().Be(new DateOnly(2026, 3, 27));
        tx.BankCode.Should().Be("0143");
        tx.BranchCode.Should().Be("001");
        tx.Amount.Should().Be(50000m);
        tx.DepositorName.Should().Be("ﾀﾅｶ ﾀﾛｳ");
        tx.BankName.Should().Be("八十二銀行");
    }

    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void TransferTransaction_OptionalFields_ShouldBeNullByDefault()
    {
        var tx = new TransferTransaction();

        tx.ConsignorName.Should().BeNull();
        tx.BankName.Should().BeNull();
        tx.BranchName.Should().BeNull();
        tx.ResultCode.Should().BeNull();
        tx.PassbookComment.Should().BeNull();
        tx.ContractorName.Should().BeNull();
        tx.TransferAccountBankCode.Should().BeNull();
        tx.TransferAccountBranchCode.Should().BeNull();
        tx.TransferAccountNo.Should().BeNull();
    }

    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void TransferTransaction_AmountFields_ShouldDefaultToZero()
    {
        var tx = new TransferTransaction();

        tx.Type1Amount.Should().Be(0m);
        tx.Type2Amount.Should().Be(0m);
        tx.Type3Amount.Should().Be(0m);
        tx.Type4Amount.Should().Be(0m);
        tx.CreditAmount.Should().Be(0m);
        tx.PreviousBalance.Should().Be(0m);
        tx.CurrentBilling.Should().Be(0m);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void TransferTransaction_ShouldSetAmountBreakdown()
    {
        var tx = new TransferTransaction
        {
            Type1Amount = 10000m,
            Type2Amount = 5000m,
            Type3Amount = 3000m,
            Type4Amount = 2000m,
            CreditAmount = 1000m,
            PreviousBalance = 500m,
            CurrentBilling = 20500m
        };

        tx.Type1Amount.Should().Be(10000m);
        tx.Type2Amount.Should().Be(5000m);
        tx.Type3Amount.Should().Be(3000m);
        tx.Type4Amount.Should().Be(2000m);
        tx.CreditAmount.Should().Be(1000m);
        tx.CurrentBilling.Should().Be(20500m);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void TransferTransaction_ShouldSetTransferAccount()
    {
        var tx = new TransferTransaction
        {
            TransferAccountBankCode = "0001",
            TransferAccountBranchCode = "001",
            TransferAccountNo = "9876543210"
        };

        tx.TransferAccountBankCode.Should().Be("0001");
        tx.TransferAccountBranchCode.Should().Be("001");
        tx.TransferAccountNo.Should().Be("9876543210");
    }

    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void TransferTransaction_CompanyNavigation_ShouldBeNullByDefault()
    {
        var tx = new TransferTransaction();

        tx.CompanyId.Should().BeNull();
        tx.Company.Should().BeNull();
    }
}










