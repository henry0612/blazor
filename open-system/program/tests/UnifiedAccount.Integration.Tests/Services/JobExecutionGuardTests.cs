using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Infrastructure.Services;
using UnifiedAccount.Integration.Tests.Helpers;

namespace UnifiedAccount.Integration.Tests.Services;

public class JobExecutionGuardTests
{
    /// <summary>このテストでは 振替サイクルを検証したとき、サイクル検証の結果が返される。</summary>
    [Fact]
    public async Task Acquire_Check_Release_Lifecycle()
    {
        using var db = TestDbContextFactory.Create();

        var bp = new BatchParameter
        {
            ParameterType = "T",
            WithdrawalDate = DateOnly.FromDateTime(DateTime.Now),
            JobExecutionStatus = "0"
        };
        db.BatchParameters.Add(bp);
        await db.SaveChangesAsync();

        var logger = NullLogger<JobExecutionGuard>.Instance;
        var guard = new JobExecutionGuard(db, logger);

        // 初期チェック: 実行可能
        var canRun = await guard.CheckAsync(bp.Id);
        canRun.Should().BeTrue();

        // 取得成功
        var acquired = await guard.AcquireAsync(bp.Id, "test");
        acquired.Should().BeTrue();

        // 取得後は実行不可
        var canRun2 = await guard.CheckAsync(bp.Id);
        canRun2.Should().BeFalse();

        // 二重取得は失敗
        var acquired2 = await guard.AcquireAsync(bp.Id, "test2");
        acquired2.Should().BeFalse();

        // 解放 (成功)
        await guard.ReleaseAsync(bp.Id, success: true);

        var refreshed = await db.BatchParameters.FindAsync(bp.Id);
        refreshed!.JobExecutionStatus.Should().Be("2");
    }
}









