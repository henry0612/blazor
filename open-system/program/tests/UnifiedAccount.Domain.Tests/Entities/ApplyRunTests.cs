using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using UnifiedAccount.Domain.Entities.Apply;

namespace UnifiedAccount.Domain.Tests.Entities;

public class ApplyRunTests
{
    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void ApplyRun_ShouldHaveDefaultValues()
    {
        var run = new ApplyRun();

        run.JobExecutionId.Should().Be(string.Empty);
        run.RunJob.Should().Be(string.Empty);
        run.AttemptNo.Should().Be(0);
        run.Status.Should().Be("RUNNING");
        run.ExecutedBy.Should().Be(string.Empty);
        run.ContractApplyCount.Should().Be(0);
        run.ContractErrorCount.Should().Be(0);
        run.AmountApplyCount.Should().Be(0);
        run.AmountErrorCount.Should().Be(0);
        run.CompletedAt.Should().BeNull();
        run.ConfirmedBy.Should().BeNull();
        run.ConfirmedAt.Should().BeNull();
    }

    /// <summary>このテストでは 属性を設定したとき、入力値が保持される。</summary>
    [Fact]
    public void ApplyRun_ShouldStoreProperties()
    {
        var run = new ApplyRun
        {
            JobExecutionId = "JOB-KOZ030-20260807-000001",
            RunJob = "JOB2",
            TransferDate = new DateOnly(2026, 8, 12),
            AttemptNo = 3,
            Status = "REVIEWING",
            ExecutedBy = "operator01",
            ContractApplyCount = 120,
            ContractErrorCount = 2,
            AmountApplyCount = 340,
            AmountErrorCount = 0,
        };

        run.RunJob.Should().Be("JOB2");
        run.TransferDate.Should().Be(new DateOnly(2026, 8, 12));
        run.AttemptNo.Should().Be(3);
        run.Status.Should().Be("REVIEWING");
        run.ContractApplyCount.Should().Be(120);
    }

    /// <summary>このテストでは JobExecutionId の最大長が 50 である。</summary>
    [Fact]
    public void JobExecutionId_MaxLength_ShouldBe50()
    {
        var prop = typeof(ApplyRun).GetProperty(nameof(ApplyRun.JobExecutionId));
        var attr = prop!.GetCustomAttributes(typeof(MaxLengthAttribute), false).Single() as MaxLengthAttribute;

        attr!.Length.Should().Be(50);
    }

    /// <summary>このテストでは ExecutedBy の最大長が 50 である。</summary>
    [Fact]
    public void ExecutedBy_MaxLength_ShouldBe50()
    {
        var prop = typeof(ApplyRun).GetProperty(nameof(ApplyRun.ExecutedBy));
        var attr = prop!.GetCustomAttributes(typeof(MaxLengthAttribute), false).Single() as MaxLengthAttribute;

        attr!.Length.Should().Be(50);
    }
}
