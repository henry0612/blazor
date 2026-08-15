using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Domain.Constants;
using UnifiedAccount.Domain.Interfaces;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Infrastructure.Services;

/// <summary>
/// DB を用いた簡易ジョブ排他制御サービス (Step7.1)
/// BatchParameters.JobExecutionStatus を 0/1/2/3 で管理する。
/// </summary>
public class JobExecutionGuard : IJobExecutionGuard
{
    /// <summary>
    /// データベースを保持する。
    /// </summary>
    private readonly AppDbContext _db;
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<JobExecutionGuard> _logger;

    public JobExecutionGuard(AppDbContext db, ILogger<JobExecutionGuard> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> CheckAsync(long batchParameterId, CancellationToken ct = default)
    {
        var bp = await _db.BatchParameters
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == batchParameterId, ct);
        if (bp is null) throw new InvalidOperationException($"BatchParameter not found: {batchParameterId}");

        return string.IsNullOrWhiteSpace(bp.JobExecutionStatus)
            || bp.JobExecutionStatus == JobExecutionStatuses.NotStarted;
    }

    public async Task<bool> AcquireAsync(long batchParameterId, string? owner = null, CancellationToken ct = default)
    {
        // トランザクションで楽観的更新を試みる
        var bp = await _db.BatchParameters.FirstOrDefaultAsync(b => b.Id == batchParameterId, ct);
        if (bp is null) throw new InvalidOperationException($"BatchParameter not found: {batchParameterId}");

        if (!string.IsNullOrWhiteSpace(bp.JobExecutionStatus)
            && bp.JobExecutionStatus != JobExecutionStatuses.NotStarted)
        {
            _logger.LogWarning("Acquire failed: BatchParameter {Id} status={Status}", bp.Id, bp.JobExecutionStatus);
            return false;
        }

        bp.JobExecutionStatus = JobExecutionStatuses.Running;
        try
        {
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Acquired BatchParameter {Id} by {Owner}", batchParameterId, owner);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Acquire concurrency conflict for BatchParameter {Id}", batchParameterId);
            return false;
        }
    }

    public async Task ReleaseAsync(long batchParameterId, bool success, CancellationToken ct = default)
    {
        var bp = await _db.BatchParameters.FirstOrDefaultAsync(b => b.Id == batchParameterId, ct);
        if (bp is null) throw new InvalidOperationException($"BatchParameter not found: {batchParameterId}");

        bp.JobExecutionStatus = success ? JobExecutionStatuses.Completed : JobExecutionStatuses.Failed;
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Released BatchParameter {Id} status={Status}", batchParameterId, bp.JobExecutionStatus);
    }
}
