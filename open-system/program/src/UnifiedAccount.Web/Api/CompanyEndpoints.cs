using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Web.Api;

/// <summary>
/// 会社マスタ API (KOZGQ20 → /api/companies) — 4エンドポイント
/// </summary>
public static class CompanyEndpoints
{
    public static RouteGroupBuilder MapCompanyEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll).WithName("GetCompanies");
        group.MapGet("/{code}", GetByCode).WithName("GetCompanyByCode");
        group.MapPost("/", Create).WithValidation<CreateCompanyRequest>().WithName("CreateCompany");
        group.MapPut("/{code}", Update).WithValidation<UpdateCompanyRequest>().WithName("UpdateCompany");
        return group;
    }

    private static async Task<IResult> GetAll(
        [AsParameters] PaginationQuery query,
        AppDbContext db,
        CancellationToken ct)
    {
        var q = db.Companies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(query.Search))
            q = q.Where(c => c.CompanyCode.Contains(query.Search)
                          || c.CompanyNameKana.Contains(query.Search));

        var totalCount = await q.CountAsync(ct);
        var items = await q
            .OrderBy(c => c.CompanyCode)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new
            {
                c.Id,
                c.CompanyCode,
                c.CompanyNameKana,
                c.CompanyNameKanji,
                c.ConsignorCode,
                c.BasicFee,
                c.AdminFee,
                c.SuspendFlag
            })
            .ToListAsync(ct);

        return Results.Ok(new PagedResult<object>(items, totalCount, query.Page, query.PageSize));
    }

    private static async Task<IResult> GetByCode(
        string code, AppDbContext db, CancellationToken ct)
    {
        var company = await db.Companies
            .AsNoTracking()
            .Include(c => c.WithdrawalDays)
            .FirstOrDefaultAsync(c => c.CompanyCode == code, ct);

        return company is null ? Results.NotFound() : Results.Ok(company);
    }

    /// <summary>
    /// CreateCompanyRequest を表すレコード。
    /// </summary>
    public record CreateCompanyRequest(
        string CompanyCode, string CompanyNameKana, string? CompanyNameKanji,
        string ConsignorCode, string? PostalCode, string? Prefecture,
        string? PhoneNumber, decimal BasicFee, decimal AdminFee);

    private static async Task<IResult> Create(
        CreateCompanyRequest req, AppDbContext db, CancellationToken ct)
    {
        if (await db.Companies.AnyAsync(c => c.CompanyCode == req.CompanyCode, ct))
            return Results.Conflict(new { message = $"会社コード '{req.CompanyCode}' は既に登録されています。" });

        var company = new Domain.Entities.Core.Company
        {
            CompanyCode = req.CompanyCode,
            CompanyNameKana = req.CompanyNameKana,
            CompanyNameKanji = req.CompanyNameKanji,
            ConsignorCode = req.ConsignorCode,
            PostalCode = req.PostalCode,
            Prefecture = req.Prefecture,
            PhoneNumber = req.PhoneNumber,
            BasicFee = req.BasicFee,
            AdminFee = req.AdminFee,
            SuspendFlag = "0"
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/companies/{company.CompanyCode}", company);
    }

    /// <summary>
    /// UpdateCompanyRequest を表すレコード。
    /// </summary>
    public record UpdateCompanyRequest(
        string? CompanyNameKana, string? CompanyNameKanji,
        string? ConsignorCode, string? PostalCode, string? Prefecture,
        string? PhoneNumber, decimal? BasicFee, decimal? AdminFee);

    private static async Task<IResult> Update(
        string code, UpdateCompanyRequest req, AppDbContext db, CancellationToken ct)
    {
        var company = await db.Companies
            .FirstOrDefaultAsync(c => c.CompanyCode == code, ct);

        if (company is null) return Results.NotFound();

        if (req.CompanyNameKana is not null) company.CompanyNameKana = req.CompanyNameKana;
        if (req.CompanyNameKanji is not null) company.CompanyNameKanji = req.CompanyNameKanji;
        if (req.ConsignorCode is not null) company.ConsignorCode = req.ConsignorCode;
        if (req.PostalCode is not null) company.PostalCode = req.PostalCode;
        if (req.Prefecture is not null) company.Prefecture = req.Prefecture;
        if (req.PhoneNumber is not null) company.PhoneNumber = req.PhoneNumber;
        if (req.BasicFee.HasValue) company.BasicFee = req.BasicFee.Value;
        if (req.AdminFee.HasValue) company.AdminFee = req.AdminFee.Value;

        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}


