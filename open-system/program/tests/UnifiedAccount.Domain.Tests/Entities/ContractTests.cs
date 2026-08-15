using FluentAssertions;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Domain.Tests.Entities;

public class ContractTests
{
    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void Contract_ShouldHaveDefaultValues()
    {
        var contract = new Contract();

        contract.CompanyCode.Should().Be(string.Empty);
        contract.PersonalCode.Should().Be(string.Empty);
        contract.CheckDigit.Should().Be(string.Empty);
        contract.CodeSave.Should().Be(" ");
        contract.SuspendFlag.Should().Be("0");
        contract.NewFlag.Should().Be("0");
        contract.ZenginFlag.Should().Be("0");
        contract.NotifiedFlag.Should().Be("0");
        contract.ResultFlag.Should().Be("0");
        contract.ProcessType.Should().Be("0");
        contract.CreditCompleteFlag.Should().Be("0");
        contract.TransferMethod.Should().Be("0");
        contract.AutoDeleteFlag.Should().Be("0");
        contract.FailureCount.Should().Be(0);
        contract.DepositorNameKana.Should().Be(string.Empty);
        contract.BankCode.Should().Be(string.Empty);
        contract.BranchCode.Should().Be(string.Empty);
        contract.AccountType.Should().Be(string.Empty);
        contract.AccountNo.Should().Be(string.Empty);
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void Contract_ShouldStoreProperties()
    {
        var contract = new Contract
        {
            CompanyCode = "TEST01",
            PersonalCode = "123456789012",
            CheckDigit = "5",
            BankCode = "0143",
            BranchCode = "001",
            AccountType = "1",
            AccountNo = "1234567890",
            DepositorNameKana = "ﾀﾅｶ ﾀﾛｳ",
            SuspendFlag = "1",
            WithdrawalDay = 27,
            CurrentBillingAmount = 10000m
        };

        contract.CompanyCode.Should().Be("TEST01");
        contract.PersonalCode.Should().Be("123456789012");
        contract.CheckDigit.Should().Be("5");
        contract.BankCode.Should().Be("0143");
        contract.BranchCode.Should().Be("001");
        contract.AccountType.Should().Be("1");
        contract.DepositorNameKana.Should().Be("ﾀﾅｶ ﾀﾛｳ");
        contract.SuspendFlag.Should().Be("1");
        contract.WithdrawalDay.Should().Be(27);
        contract.CurrentBillingAmount.Should().Be(10000m);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void Contract_NavigationCollections_ShouldBeEmpty()
    {
        var contract = new Contract();

        contract.Types.Should().NotBeNull().And.BeEmpty();
        contract.BillingAmounts.Should().NotBeNull().And.BeEmpty();
        contract.TransferFailures.Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void Contract_OptionalFields_ShouldBeNullByDefault()
    {
        var contract = new Contract();

        contract.StartYearMonth.Should().BeNull();
        contract.ContractorNameKana.Should().BeNull();
        contract.DepositorNameKanji.Should().BeNull();
        contract.ContractorNameKanji.Should().BeNull();
        contract.PostalCode.Should().BeNull();
        contract.PhoneNumber.Should().BeNull();
        contract.CreditProductName.Should().BeNull();
        contract.CreditTotalAmount.Should().BeNull();
        contract.WithdrawalDate.Should().BeNull();
        contract.BankProcessDate.Should().BeNull();
        contract.ExemptionDate.Should().BeNull();
        contract.ChangeDate.Should().BeNull();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void Contract_ShouldSetForeignKeys()
    {
        var contract = new Contract
        {
            CompanyId = 42,
            BankBranchId = 99
        };

        contract.CompanyId.Should().Be(42);
        contract.BankBranchId.Should().Be(99);
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void Contract_CreditFields_ShouldStoreValues()
    {
        var contract = new Contract
        {
            CreditTotalAmount = 120000m,
            CreditTotalCount = 12,
            CreditCompletedCount = 6,
            CreditPayment1 = 10000m,
            CreditPayment2 = 5000m,
            CreditSpecialAddition = 2000m,
            CreditBillingAmount = 12000m
        };

        contract.CreditTotalAmount.Should().Be(120000m);
        contract.CreditTotalCount.Should().Be(12);
        contract.CreditCompletedCount.Should().Be(6);
        contract.CreditPayment1.Should().Be(10000m);
        contract.CreditBillingAmount.Should().Be(12000m);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void Contract_BaseEntity_ShouldHaveIdAndTimestamps()
    {
        var contract = new Contract();

        contract.Id.Should().Be(0);
        contract.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        contract.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }
}









