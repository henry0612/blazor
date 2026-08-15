using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Web.Api;

/// <summary>
/// 全銀 API (KOZ400系 → /api/zengin) — 4エンドポイント
/// </summary>
public static class ZenginEndpoints
{
    public static RouteGroupBuilder MapZenginEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/batches", GetBatches).WithName("GetZenginBatches");
        group.MapGet("/batches/{id}", GetBatchById).WithName("GetZenginBatchById");
        group.MapPost("/send", Send).WithValidation<SendRequest>().WithName("ZenginSend");
        group.MapPost("/receive", Receive).WithValidation<ReceiveRequest>().WithName("ZenginReceive");
        return group;
    }

    /// <summary>
    /// ZenginSearchQuery を表すレコード。
    /// </summary>
    public record ZenginSearchQuery(
        int Page = 1, int PageSize = 20,
        string? ProcessDate = null, string? ConsignorCode = null);

    private static async Task<IResult> GetBatches(
        [AsParameters] ZenginSearchQuery query, AppDbContext db, CancellationToken ct)
    {
        var q = db.ZenginBatches.AsNoTracking().AsQueryable();

        if (DateOnly.TryParse(query.ProcessDate, out var date))
            q = q.Where(b => b.WithdrawalDate == date);

        if (!string.IsNullOrEmpty(query.ConsignorCode))
            q = q.Where(b => b.ConsignorCode == query.ConsignorCode);

        var totalCount = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(b => b.WithdrawalDate)
            .ThenBy(b => b.ConsignorCode)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => new
            {
                b.Id,
                b.TypeCode,
                b.ConsignorCode,
                b.ConsignorName,
                b.WithdrawalDate,
                b.TotalCount,
                b.TotalAmount,
                b.SettledCount,
                b.SettledAmount,
                b.FailedCount,
                b.FailedAmount
            })
            .ToListAsync(ct);

        return Results.Ok(new PagedResult<object>(items, totalCount, query.Page, query.PageSize));
    }

    private static async Task<IResult> GetBatchById(
        long id, AppDbContext db, CancellationToken ct)
    {
        var batch = await db.ZenginBatches
            .AsNoTracking()
            .Include(b => b.Transactions)
            .Include(b => b.TransmissionLogs)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (batch is null) return Results.NotFound();

        return Results.Ok(new
        {
            batch.Id,
            batch.TypeCode,
            batch.ConsignorCode,
            batch.ConsignorName,
            batch.WithdrawalDate,
            batch.TotalCount,
            batch.TotalAmount,
            batch.SettledCount,
            batch.SettledAmount,
            batch.FailedCount,
            batch.FailedAmount,
            Transactions = batch.Transactions.Select(t => new
            {
                t.Id,
                t.BankCode,
                t.BankName,
                t.BranchCode,
                t.BranchName,
                t.AccountType,
                t.AccountNo,
                t.DepositorName,
                t.Amount,
                t.NewCode,
                t.ResultCode,
                t.ContractorCode
            }),
            TransmissionLogs = batch.TransmissionLogs.Select(l => new
            {
                l.Id,
                l.ManagementDate,
                l.ManagementTime,
                l.DeliveryDate,
                l.DeliveryTime,
                l.DeliveryFlag,
                l.RecordCount
            })
        });
    }

    /// <summary>
    /// SendRequest を表すレコード。
    /// </summary>
    public record SendRequest(DateOnly WithdrawalDate, string? ConsignorCode = null);

    private static async Task<IResult> Send(
        SendRequest req, AppDbContext db, CancellationToken ct)
    {
        // 送信対象バッチを検索
        var q = db.ZenginBatches
            .Include(b => b.TransmissionLogs)
            .Where(b => b.WithdrawalDate == req.WithdrawalDate);

        if (!string.IsNullOrEmpty(req.ConsignorCode))
            q = q.Where(b => b.ConsignorCode == req.ConsignorCode);

        var batches = await q.ToListAsync(ct);

        if (batches.Count == 0)
            return Results.NotFound(new { message = "送信対象バッチがありません。" });

        int sentCount = 0;
        foreach (var batch in batches)
        {
            // 伝送ログ追加
            db.ZenginTransmissionLogs.Add(new Domain.Entities.Zengin.ZenginTransmissionLog
            {
                ZenginBatchId = batch.Id,
                ManagementDate = DateOnly.FromDateTime(LocalDateTimeProvider.Now),
                ManagementTime = TimeOnly.FromDateTime(LocalDateTimeProvider.Now),
                WithdrawalMonth = req.WithdrawalDate.Month.ToString("D2"),
                WithdrawalDay = req.WithdrawalDate.Day.ToString("D2"),
                RecordCount = batch.TotalCount ?? 0,
                DeliveryDate = DateOnly.FromDateTime(LocalDateTimeProvider.Now),
                DeliveryTime = TimeOnly.FromDateTime(LocalDateTimeProvider.Now),
                DeliveryFlag = "1" // 送信済
            });
            sentCount++;
        }

        await db.SaveChangesAsync(ct);
        return Results.Ok(new { sentCount, message = $"{sentCount}バッチ送信しました。" });
    }

    /// <summary>
    /// ReceiveRequest を表すレコード。
    /// </summary>
    public record ReceiveRequest(DateOnly WithdrawalDate);

    private static async Task<IResult> Receive(
        ReceiveRequest req, AppDbContext db, CancellationToken ct)
    {
        // 送信済みバッチの結果受信 (シミュレーション)
        var batches = await db.ZenginBatches
            .Include(b => b.Transactions)
            .Where(b => b.WithdrawalDate == req.WithdrawalDate && b.SettledCount == null)
            .ToListAsync(ct);

        if (batches.Count == 0)
            return Results.NotFound(new { message = "受信対象バッチがありません。" });

        int totalSettled = 0, totalFailed = 0;
        foreach (var batch in batches)
        {
            foreach (var txn in batch.Transactions.Where(t => t.ResultCode == null))
            {
                txn.ResultCode = "0"; // 仮: 全件成功
                totalSettled++;
            }

            batch.SettledCount = batch.Transactions.Count(t => t.ResultCode == "0");
            batch.SettledAmount = batch.Transactions.Where(t => t.ResultCode == "0").Sum(t => t.Amount);
            batch.FailedCount = batch.Transactions.Count(t => t.ResultCode != "0");
            batch.FailedAmount = batch.Transactions.Where(t => t.ResultCode != "0").Sum(t => t.Amount);
        }

        await db.SaveChangesAsync(ct);
        return Results.Ok(new { batchCount = batches.Count, totalSettled, totalFailed });
    }
}
