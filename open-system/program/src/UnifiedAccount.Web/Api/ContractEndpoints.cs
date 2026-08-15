using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Web.Api;

/// <summary>
/// 契約者マスタ API (KOZGQ30/40, KOZGR10 → /api/contracts) — 5エンドポイント
/// </summary>
public static class ContractEndpoints
{
    public static RouteGroupBuilder MapContractEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll).WithName("GetContracts");
        group.MapGet("/{companyCode}/{personalCode}", GetByKey).WithName("GetContractByKey");
        group.MapPost("/", Create).WithValidation<CreateContractRequest>().WithName("CreateContract");
        group.MapPut("/{companyCode}/{personalCode}", Update).WithValidation<UpdateContractRequest>().WithName("UpdateContract");
        group.MapDelete("/{companyCode}/{personalCode}", Delete).WithName("DeleteContract");
        return group;
    }

    /// <summary>
    /// ContractSearchQuery を表すレコード。
    /// </summary>
    public record ContractSearchQuery(
        int Page = 1, int PageSize = 20,
        int SearchType = 1,
        string? CompanyCode = null, string? PersonalCode = null,
        string? BankCode = null, string? BranchCode = null,
        string? AccountNo = null,
        string? DepositorNameFrom = null, string? DepositorNameTo = null,
        string? SuspendFlag = null);

    private static async Task<IResult> GetAll(
        [AsParameters] ContractSearchQuery query, AppDbContext db, CancellationToken ct)
    {
        var q = db.Contracts.AsNoTracking().AsQueryable();

        // 4キー検索戦略 (現行 KSDSKV/KV1/KV2/KV3)
        q = query.SearchType switch
        {
            1 => q.Where(c => (query.CompanyCode == null || c.CompanyCode == query.CompanyCode)
                           && (query.PersonalCode == null || c.PersonalCode.StartsWith(query.PersonalCode))),
            2 => q.Where(c => (query.CompanyCode == null || c.CompanyCode == query.CompanyCode)
                           && (query.AccountNo == null || c.AccountNo == query.AccountNo)),
            3 => q.Where(c => (query.BankCode == null || c.BankCode == query.BankCode)
                           && (query.BranchCode == null || c.BranchCode == query.BranchCode)
                           && (query.AccountNo == null || c.AccountNo == query.AccountNo)),
            4 => q.Where(c => (query.DepositorNameFrom == null || c.DepositorNameKana.CompareTo(query.DepositorNameFrom) >= 0)
                           && (query.DepositorNameTo == null || c.DepositorNameKana.CompareTo(query.DepositorNameTo) <= 0)),
            _ => q
        };

        if (!string.IsNullOrEmpty(query.SuspendFlag))
            q = q.Where(c => c.SuspendFlag == query.SuspendFlag);

        var totalCount = await q.CountAsync(ct);
        var items = await q
            .OrderBy(c => c.CompanyCode).ThenBy(c => c.PersonalCode)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new
            {
                c.Id,
                c.CompanyCode,
                c.PersonalCode,
                c.CheckDigit,
                c.DepositorNameKana,
                c.DepositorNameKanji,
                c.BankCode,
                c.BranchCode,
                c.AccountType,
                c.AccountNo,
                c.WithdrawalDay,
                c.CurrentBillingAmount,
                c.SuspendFlag
            })
            .ToListAsync(ct);

        return Results.Ok(new PagedResult<object>(items, totalCount, query.Page, query.PageSize));
    }

    private static async Task<IResult> GetByKey(
        string companyCode, string personalCode, AppDbContext db, CancellationToken ct)
    {
        var contract = await db.Contracts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CompanyCode == companyCode && c.PersonalCode == personalCode, ct);

        return contract is null ? Results.NotFound() : Results.Ok(contract);
    }

    /// <summary>
    /// CreateContractRequest を表すレコード。
    /// </summary>
    public record CreateContractRequest(
        string CompanyCode, string PersonalCode, string CheckDigit,
        string DepositorNameKana, string? DepositorNameKanji,
        string BankCode, string BranchCode, string AccountType, string AccountNo,
        short WithdrawalDay, decimal CurrentBillingAmount);

    private static async Task<IResult> Create(
        CreateContractRequest req, AppDbContext db, CancellationToken ct)
    {
        if (await db.Contracts.AnyAsync(c => c.CompanyCode == req.CompanyCode && c.PersonalCode == req.PersonalCode, ct))
            return Results.Conflict(new { message = "同一契約者が既に登録されています。" });

        var contract = new Domain.Entities.Core.Contract
        {
            CompanyCode = req.CompanyCode,
            PersonalCode = req.PersonalCode,
            CheckDigit = req.CheckDigit,
            DepositorNameKana = req.DepositorNameKana,
            DepositorNameKanji = req.DepositorNameKanji,
            BankCode = req.BankCode,
            BranchCode = req.BranchCode,
            AccountType = req.AccountType,
            AccountNo = req.AccountNo,
            WithdrawalDay = req.WithdrawalDay,
            CurrentBillingAmount = req.CurrentBillingAmount,
            SuspendFlag = "0"
        };
        db.Contracts.Add(contract);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/contracts/{contract.CompanyCode}/{contract.PersonalCode}", contract);
    }

    /// <summary>
    /// UpdateContractRequest を表すレコード。
    /// </summary>
    public record UpdateContractRequest(
        string? DepositorNameKana, string? DepositorNameKanji,
        string? BankCode, string? BranchCode, string? AccountType, string? AccountNo,
        short? WithdrawalDay, decimal? CurrentBillingAmount, string? SuspendFlag);

    private static async Task<IResult> Update(
        string companyCode, string personalCode, UpdateContractRequest req,
        AppDbContext db, CancellationToken ct)
    {
        var contract = await db.Contracts
            .FirstOrDefaultAsync(c => c.CompanyCode == companyCode && c.PersonalCode == personalCode, ct);

        if (contract is null) return Results.NotFound();

        if (req.DepositorNameKana is not null) contract.DepositorNameKana = req.DepositorNameKana;
        if (req.DepositorNameKanji is not null) contract.DepositorNameKanji = req.DepositorNameKanji;
        if (req.BankCode is not null) contract.BankCode = req.BankCode;
        if (req.BranchCode is not null) contract.BranchCode = req.BranchCode;
        if (req.AccountType is not null) contract.AccountType = req.AccountType;
        if (req.AccountNo is not null) contract.AccountNo = req.AccountNo;
        if (req.WithdrawalDay.HasValue) contract.WithdrawalDay = req.WithdrawalDay.Value;
        if (req.CurrentBillingAmount.HasValue) contract.CurrentBillingAmount = req.CurrentBillingAmount.Value;
        if (req.SuspendFlag is not null) contract.SuspendFlag = req.SuspendFlag;

        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Delete(
        string companyCode, string personalCode, AppDbContext db, CancellationToken ct)
    {
        var contract = await db.Contracts
            .FirstOrDefaultAsync(c => c.CompanyCode == companyCode && c.PersonalCode == personalCode, ct);

        if (contract is null) return Results.NotFound();

        db.Contracts.Remove(contract);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}


