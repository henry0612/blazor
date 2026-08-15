using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Web.Api;

/// <summary>
/// 振替 API (KOZGR35/65, KOZ200系 → /api/transfers) — 5エンドポイント
/// </summary>
public static class TransferEndpoints
{
    public static RouteGroupBuilder MapTransferEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetTransactions).WithName("GetTransferTransactions");
        group.MapGet("/failures", GetFailures).WithName("GetTransferFailures");
        group.MapPost("/failures/{id}/clear", ClearFailure).WithName("ClearTransferFailure");
        return group;
    }

    /// <summary>
    /// TransferSearchQuery を表すレコード。
    /// </summary>
    public record TransferSearchQuery(
        int Page = 1, int PageSize = 20,
        string? CompanyCode = null, string? ProcessDate = null);

    private static async Task<IResult> GetTransactions(
        [AsParameters] TransferSearchQuery query, AppDbContext db, CancellationToken ct)
    {
        var q = db.TransferTransactions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(query.CompanyCode))
            q = q.Where(t => t.CompanyCode == query.CompanyCode);

        if (DateOnly.TryParse(query.ProcessDate, out var date))
            q = q.Where(t => t.WithdrawalDate == date);

        var totalCount = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(t => t.WithdrawalDate)
            .ThenBy(t => t.CompanyCode)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new
            {
                t.Id,
                t.CompanyCode,
                t.PersonalCode,
                t.ConsignorCode,
                t.BankCode,
                t.BranchCode,
                t.AccountNo,
                t.DepositorName,
                t.Amount,
                t.WithdrawalDate,
                t.ResultCode
            })
            .ToListAsync(ct);

        return Results.Ok(new PagedResult<object>(items, totalCount, query.Page, query.PageSize));
    }

    /// <summary>
    /// FailureSearchQuery を表すレコード。
    /// </summary>
    public record FailureSearchQuery(
        int Page = 1, int PageSize = 20,
        string? CompanyCode = null, string? TransferType = null);

    private static async Task<IResult> GetFailures(
        [AsParameters] FailureSearchQuery query, AppDbContext db, CancellationToken ct)
    {
        var q = db.TransferFailures.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(query.CompanyCode))
            q = q.Where(f => f.CompanyCode == query.CompanyCode);

        if (!string.IsNullOrEmpty(query.TransferType))
            q = q.Where(f => f.TransferType == query.TransferType);

        var totalCount = await q.CountAsync(ct);
        var items = await q
            .OrderBy(f => f.CompanyCode).ThenBy(f => f.PersonalCode)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(f => new
            {
                f.Id,
                f.CompanyCode,
                f.PersonalCode,
                f.TransferType,
                f.ResultCode,
                f.BankCode
            })
            .ToListAsync(ct);

        return Results.Ok(new PagedResult<object>(items, totalCount, query.Page, query.PageSize));
    }

    private static async Task<IResult> ClearFailure(
        long id, AppDbContext db, CancellationToken ct)
    {
        var failure = await db.TransferFailures.FindAsync(new object[] { id }, ct);
        if (failure is null) return Results.NotFound();

        db.TransferFailures.Remove(failure);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

}

