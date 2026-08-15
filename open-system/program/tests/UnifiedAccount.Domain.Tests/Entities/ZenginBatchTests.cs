using FluentAssertions;
using UnifiedAccount.Domain.Entities.Zengin;

namespace UnifiedAccount.Domain.Tests.Entities;

public class ZenginBatchTests
{
    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void ZenginBatch_ShouldHaveDefaultValues()
    {
        var batch = new ZenginBatch();

        batch.TypeCode.Should().Be("91");
        batch.ConsignorCode.Should().Be(string.Empty);
        batch.ConsignorName.Should().Be(string.Empty);
        batch.WithdrawalDate.Should().Be(default);
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void ZenginBatch_ShouldStoreProperties()
    {
        var batch = new ZenginBatch
        {
            TypeCode = "91",
            ConsignorCode = "1234567890",
            ConsignorName = "テスト委託者カナ",
            WithdrawalDate = new DateOnly(2026, 3, 27),
            TransferBankCode = "0143",
            TransferBranchCode = "001",
            TransferAccountNo = "1234567890"
        };

        batch.ConsignorCode.Should().Be("1234567890");
        batch.ConsignorName.Should().Be("テスト委託者カナ");
        batch.WithdrawalDate.Should().Be(new DateOnly(2026, 3, 27));
        batch.TransferBankCode.Should().Be("0143");
        batch.TransferBranchCode.Should().Be("001");
        batch.TransferAccountNo.Should().Be("1234567890");
    }

    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void ZenginBatch_OptionalFields_ShouldBeNullByDefault()
    {
        var batch = new ZenginBatch();

        batch.TransferBankCode.Should().BeNull();
        batch.TransferBranchCode.Should().BeNull();
        batch.TransferAccountNo.Should().BeNull();
        batch.TotalCount.Should().BeNull();
        batch.TotalAmount.Should().BeNull();
        batch.SettledCount.Should().BeNull();
        batch.SettledAmount.Should().BeNull();
        batch.FailedCount.Should().BeNull();
        batch.FailedAmount.Should().BeNull();
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void ZenginBatch_ShouldStoreTotals()
    {
        var batch = new ZenginBatch
        {
            TotalCount = 100,
            TotalAmount = 5000000m,
            SettledCount = 95,
            SettledAmount = 4750000m,
            FailedCount = 5,
            FailedAmount = 250000m
        };

        batch.TotalCount.Should().Be(100);
        batch.TotalAmount.Should().Be(5000000m);
        batch.SettledCount.Should().Be(95);
        batch.SettledAmount.Should().Be(4750000m);
        batch.FailedCount.Should().Be(5);
        batch.FailedAmount.Should().Be(250000m);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void ZenginBatch_NavigationCollections_ShouldBeEmpty()
    {
        var batch = new ZenginBatch();

        batch.Transactions.Should().NotBeNull().And.BeEmpty();
        batch.TransmissionLogs.Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void ZenginBatch_BaseEntity_ShouldHaveIdAndTimestamps()
    {
        var batch = new ZenginBatch();

        batch.Id.Should().Be(0);
        batch.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        batch.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }
}









