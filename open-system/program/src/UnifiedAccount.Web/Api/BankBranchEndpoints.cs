using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Interfaces;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Web.Api;

/// <summary>
/// 銀行支店マスタ API (KOZGQ10 → /api/bank-branches)。
/// </summary>
public static class BankBranchEndpoints
{
    public static RouteGroupBuilder MapBankBranchEndpoints(this RouteGroupBuilder group)
    {
        // ルートグループ側で RequireAuthorization() 済み (Program.cs)。
        // F-ONL-007 v1.4 は画面固有の認可ポリシーを持たないため、ここでは個別のポリシーを要求しない。
        group.MapGet("/", GetAll)
            .WithName("GetBankBranches");
        group.MapGet("/{bankCode}/{branchCode}", GetByKey)
            .WithName("GetBankBranchByKey");
        group.MapPost("/", Create)
            .WithValidation<CreateBankBranchRequest>()
            .WithName("CreateBankBranch");
        group.MapPut("/{bankCode}/{branchCode}", Update)
            .WithValidation<UpdateBankBranchRequest>()
            .WithName("UpdateBankBranch");
        group.MapDelete("/{bankCode}/{branchCode}", Delete)
            .WithName("DeleteBankBranch");
        return group;
    }

    private static async Task<IResult> GetAll(
        [AsParameters] BankBranchListQuery query,
        AppDbContext db,
        CancellationToken ct)
    {
        var queryErrors = ValidateQuery(query);
        if (queryErrors.Count > 0)
            return Results.ValidationProblem(queryErrors);

        var branches = db.BankBranches.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(query.BankCode))
            branches = branches.Where(branch => branch.BankCode.Contains(query.BankCode));
        if (!string.IsNullOrEmpty(query.BranchCode))
            branches = branches.Where(branch => branch.BranchCode.Contains(query.BranchCode));

        var totalCount = await branches.CountAsync(ct);
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)query.PageSize);
        var items = await branches
            .OrderBy(branch => branch.BankCode)
            .ThenBy(branch => branch.BranchCode)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(branch => new BankBranchRow(
                branch.Id,
                branch.BankCode,
                branch.BranchCode,
                branch.BankNameKana,
                branch.BranchNameKana,
                branch.BankNameKanji,
                branch.BranchNameKanji,
                branch.UpdatedAt))
            .ToListAsync(ct);

        return Results.Ok(new BankBranchListResponse(
            items,
            totalCount,
            query.Page,
            query.PageSize,
            totalPages,
            query.Page > 1 && totalPages > 0,
            query.Page < totalPages));
    }

    private static async Task<IResult> GetByKey(
        string bankCode,
        string branchCode,
        AppDbContext db,
        CancellationToken ct)
    {
        var branch = await db.BankBranches
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.BankCode == bankCode && item.BranchCode == branchCode,
                ct);

        return branch is null ? Results.NotFound() : Results.Ok(branch);
    }

    /// <summary>
    /// 新規登録要求。
    /// </summary>
    public record CreateBankBranchRequest(
        string BankCode,
        string BranchCode,
        string BankNameKana,
        string BranchNameKana,
        string? BankNameKanji,
        string? BranchNameKanji);

    private static async Task<IResult> Create(
        [FromBody] CreateBankBranchRequest request,
        AppDbContext db,
        ICharacterConverter converter,
        ILoggerFactory loggerFactory,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (await db.BankBranches.AnyAsync(
                branch => branch.BankCode == request.BankCode && branch.BranchCode == request.BranchCode,
                ct))
        {
            return Results.Conflict(new { message = "同一銀行支店が既に登録されています。" });
        }

        var normalized = NormalizeKana(request.BankNameKana, request.BranchNameKana, converter);
        if (!normalized.IsValid)
            return Results.ValidationProblem(normalized.Errors);

        var branch = new Domain.Entities.Core.BankBranch
        {
            BankCode = request.BankCode,
            BranchCode = request.BranchCode,
            BankNameKana = normalized.BankNameKana,
            BranchNameKana = normalized.BranchNameKana,
            BankNameKanji = request.BankNameKanji,
            BranchNameKanji = request.BranchNameKanji,
            KanjiSetFlag = HasKanji(request.BankNameKanji, request.BranchNameKanji) ? "1" : "0"
        };

        db.BankBranches.Add(branch);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception)
        {
            LogFailure(loggerFactory, httpContext, "Create", request.BankCode, request.BranchCode, exception);
            return Results.Conflict(new { message = "同一銀行支店が既に登録されています。" });
        }

        return Results.Created(
            $"/api/bank-branches/{branch.BankCode}/{branch.BranchCode}",
            branch);
    }

    /// <summary>
    /// 更新要求。
    /// </summary>
    public record UpdateBankBranchRequest(
        string BankNameKana,
        string BranchNameKana,
        string? BankNameKanji,
        string? BranchNameKanji,
        DateTime OriginalUpdatedAt);

    private static async Task<IResult> Update(
        string bankCode,
        string branchCode,
        [FromBody] UpdateBankBranchRequest request,
        AppDbContext db,
        ICharacterConverter converter,
        ILoggerFactory loggerFactory,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var branch = await db.BankBranches
            .FirstOrDefaultAsync(
                item => item.BankCode == bankCode && item.BranchCode == branchCode,
                ct);
        if (branch is null)
            return Results.NotFound();

        if (branch.UpdatedAt != request.OriginalUpdatedAt)
            return Results.Conflict(new { message = "他のユーザーが更新しました。再読込してから編集してください。" });

        var normalized = NormalizeKana(request.BankNameKana, request.BranchNameKana, converter);
        if (!normalized.IsValid)
            return Results.ValidationProblem(normalized.Errors);

        branch.BankNameKana = normalized.BankNameKana;
        branch.BranchNameKana = normalized.BranchNameKana;
        branch.BankNameKanji = request.BankNameKanji;
        branch.BranchNameKanji = request.BranchNameKanji;
        branch.KanjiSetFlag = HasKanji(request.BankNameKanji, request.BranchNameKanji) ? "1" : "0";

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            LogFailure(loggerFactory, httpContext, "Update", bankCode, branchCode, exception);
            return Results.Conflict(new { message = "他のユーザーが更新しました。再読込してから編集してください。" });
        }

        return Results.NoContent();
    }

    private static async Task<IResult> Delete(
        string bankCode,
        string branchCode,
        [FromBody] DeleteBankBranchRequest request,
        AppDbContext db,
        ILoggerFactory loggerFactory,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var branch = await db.BankBranches
            .FirstOrDefaultAsync(
                item => item.BankCode == bankCode && item.BranchCode == branchCode,
                ct);
        if (branch is null)
            return Results.NotFound();

        if (branch.UpdatedAt != request.OriginalUpdatedAt)
            return Results.Conflict(new { message = "他のユーザーが更新しました。再読込してから編集してください。" });

        db.BankBranches.Remove(branch);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            LogFailure(loggerFactory, httpContext, "Delete", bankCode, branchCode, exception);
            return Results.Conflict(new { message = "他のユーザーが更新しました。再読込してから編集してください。" });
        }

        return Results.NoContent();
    }

    /// <summary>
    /// 削除要求。
    /// </summary>
    public record DeleteBankBranchRequest(DateTime OriginalUpdatedAt);

    private static Dictionary<string, string[]> ValidateQuery(BankBranchListQuery query)
    {
        var errors = new Dictionary<string, string[]>();
        if (query.Page < 1)
            errors["Page"] = ["ページ番号は1以上で指定してください。"];
        if (query.PageSize is < 1 or > 50)
            errors["PageSize"] = ["ページサイズは1から50の範囲で指定してください。"];
        if (query.BankCode is not null && (query.BankCode.Length > 4 || !IsDigits(query.BankCode)))
            errors["BankCode"] = ["金融機関コードは4桁以内の数字で指定してください。"];
        if (query.BranchCode is not null && (query.BranchCode.Length > 3 || !IsDigits(query.BranchCode)))
            errors["BranchCode"] = ["支店コードは3桁以内の数字で指定してください。"];
        return errors;
    }

    private static (bool IsValid, string BankNameKana, string BranchNameKana, Dictionary<string, string[]> Errors)
        NormalizeKana(
            string bankNameKana,
            string branchNameKana,
            ICharacterConverter converter)
    {
        var errors = new Dictionary<string, string[]>();
        var bank = converter.NormalizeZenginKana(bankNameKana);
        var branch = converter.NormalizeZenginKana(branchNameKana);
        if (!bank.IsValid)
            errors["BankNameKana"] = bank.Errors.Select(e => e.Message).ToArray();
        if (!branch.IsValid)
            errors["BranchNameKana"] = branch.Errors.Select(e => e.Message).ToArray();

        return (
            errors.Count == 0,
            bank.NormalizedValue ?? string.Empty,
            branch.NormalizedValue ?? string.Empty,
            errors);
    }

    private static bool IsDigits(string value) => value.All(char.IsAsciiDigit);

    private static bool HasKanji(string? bankNameKanji, string? branchNameKanji) =>
        !string.IsNullOrEmpty(bankNameKanji) || !string.IsNullOrEmpty(branchNameKanji);

    private static void LogFailure(
        ILoggerFactory loggerFactory,
        HttpContext httpContext,
        string operation,
        string bankCode,
        string branchCode,
        Exception exception)
    {
        var logger = loggerFactory.CreateLogger(typeof(BankBranchEndpoints));
        logger.LogError(
            exception,
            "銀行支店API処理失敗 CorrelationId={CorrelationId} UserId={UserId} Operation={Operation} BankCode={BankCode} BranchCode={BranchCode}",
            httpContext.TraceIdentifier,
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
            operation,
            bankCode,
            branchCode);
    }
}
