using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Application.Services;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Domain.ValueObjects;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Infrastructure.Services;

/// <summary>
/// 銀行支店マスター サービス
/// </summary>
public class BankBranchService : IBankBranchService
{
    private const string FeatureId = "F-ONL-007";
    private const string TargetType = "BankBranch";

    // 既定のSystem.Text.Jsonエンコーダーは非ASCII文字を\uXXXXへエスケープするため、
    // カナ名を含む監査値JSONがTD_AuditRecords上で人間に読めなくなる。全銀許可文字は
    // 半角カナと一部記号だけで機密情報を含まないため、緩和エンコーダーで可読な形に保つ。
    private static readonly JsonSerializerOptions AuditJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly AppDbContext _context;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IAuditLogService _auditLogService;

    // AuditLogServiceと同一スコープのAppDbContextを直接注入する。
    // IDbContextFactoryでメソッドごとに別インスタンスを作ると、監査書込みと業務保存が
    // 同一トランザクションに参加できず、223 §6.4.1の「いずれかが失敗したら全体をロールバック」を満たせない。
    public BankBranchService(
        AppDbContext context,
        IDbContextFactory<AppDbContext> dbContextFactory,
        IAuditLogService auditLogService)
    {
        _context = context;
        _dbContextFactory = dbContextFactory;
        _auditLogService = auditLogService;
    }

    /// <inheritdoc />
    public async Task<PagedResult<BankBranchRow>> SearchAsync(
        string? bankCode,
        string? branchCode,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        // QuickGrid は同時に複数回 ItemsProvider を呼び出すことがあるため、
        // 読み取りは都度 DbContext を生成してスレッド競合を回避する。
        await using var readContext = await _dbContextFactory.CreateDbContextAsync(ct);
        var query = readContext.BankBranches.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(bankCode))
        {
            var term = bankCode.Trim();
            query = query.Where(x => x.BankCode.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            var term = branchCode.Trim();
            query = query.Where(x => x.BranchCode.Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.BankCode)
            .ThenBy(x => x.BranchCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new BankBranchRow(
                x.Id,
                x.BankCode,
                x.BranchCode,
                x.BankNameKana,
                x.BranchNameKana,
                x.BankNameKanji,
                x.BranchNameKanji,
                x.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<BankBranchRow>(items, totalCount, page, pageSize);
    }

    /// <inheritdoc />
    public async Task<BankBranchDto?> GetByKeyAsync(
        string bankCode,
        string branchCode,
        CancellationToken ct = default)
    {
        await using var readContext = await _dbContextFactory.CreateDbContextAsync(ct);
        var entity = await readContext.BankBranches
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BankCode == bankCode && x.BranchCode == branchCode, ct);

        return entity is null ? null : ToDto(entity);
    }

    /// <inheritdoc />
    public async Task<CreateResult> CreateAsync(
        CreateBankBranchRequest request,
        CancellationToken ct = default)
    {
        var errors = ValidateRequest(request.BankCode, request.BranchCode,
            request.BankNameKana, request.BranchNameKana);
        if (errors.Count > 0)
            return new CreateResult.ValidationError(errors);

        var exists = await _context.BankBranches
            .AnyAsync(x => x.BankCode == request.BankCode && x.BranchCode == request.BranchCode, ct);
        if (exists)
            return new CreateResult.Duplicate();

        var entity = new BankBranch
        {
            BankCode = request.BankCode,
            BranchCode = request.BranchCode,
            BankNameKana = request.BankNameKana,
            BranchNameKana = request.BranchNameKana,
            BankNameKanji = request.BankNameKanji,
            BranchNameKanji = request.BranchNameKanji,
            KanjiSetFlag = string.IsNullOrWhiteSpace(request.BankNameKanji) &&
                           string.IsNullOrWhiteSpace(request.BranchNameKanji) ? "0" : "1"
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            _context.BankBranches.Add(entity);
            await _context.SaveChangesAsync(ct);

            await _auditLogService.WriteAsync(
                BuildAuditEntry(
                    "Save",
                    request.BankCode,
                    request.BranchCode,
                    beforeValuesJson: "{}",
                    afterValuesJson: BuildKanaJson(entity.BankNameKana, entity.BranchNameKana),
                    request.ActorId,
                    request.CorrelationId),
                ct);

            await transaction.CommitAsync(ct);
            return new CreateResult.Success(ToDto(entity));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return new CreateResult.DbError(ex);
        }
    }

    /// <inheritdoc />
    public async Task<UpdateResult> UpdateAsync(
        string bankCode,
        string branchCode,
        UpdateBankBranchRequest request,
        CancellationToken ct = default)
    {
        var entity = await _context.BankBranches
            .FirstOrDefaultAsync(x => x.BankCode == bankCode && x.BranchCode == branchCode, ct);

        if (entity is null)
            return new UpdateResult.NotFound();

        if (entity.UpdatedAt != request.OriginalUpdatedAt)
            return new UpdateResult.Conflict();

        var beforeValuesJson = BuildKanaJson(entity.BankNameKana, entity.BranchNameKana);

        entity.BankNameKana = request.BankNameKana;
        entity.BranchNameKana = request.BranchNameKana;
        entity.BankNameKanji = request.BankNameKanji;
        entity.BranchNameKanji = request.BranchNameKanji;
        entity.KanjiSetFlag = string.IsNullOrWhiteSpace(request.BankNameKanji) &&
                              string.IsNullOrWhiteSpace(request.BranchNameKanji) ? "0" : "1";
        entity.UpdatedAt = DateTime.UtcNow;

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await _context.SaveChangesAsync(ct);

            await _auditLogService.WriteAsync(
                BuildAuditEntry(
                    "Save",
                    bankCode,
                    branchCode,
                    beforeValuesJson,
                    afterValuesJson: BuildKanaJson(entity.BankNameKana, entity.BranchNameKana),
                    request.ActorId,
                    request.CorrelationId),
                ct);

            await transaction.CommitAsync(ct);
            return new UpdateResult.Success(ToDto(entity));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return new UpdateResult.DbError(ex);
        }
    }

    /// <inheritdoc />
    public async Task<DeleteResult> DeleteAsync(
        string bankCode,
        string branchCode,
        DeleteBankBranchRequest request,
        CancellationToken ct = default)
    {
        var entity = await _context.BankBranches
            .FirstOrDefaultAsync(x => x.BankCode == bankCode && x.BranchCode == branchCode, ct);

        if (entity is null)
            return new DeleteResult.NotFound();

        if (entity.UpdatedAt != request.OriginalUpdatedAt)
            return new DeleteResult.Conflict();

        var beforeValuesJson = BuildKanaJson(entity.BankNameKana, entity.BranchNameKana);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            _context.BankBranches.Remove(entity);
            await _context.SaveChangesAsync(ct);

            await _auditLogService.WriteAsync(
                BuildAuditEntry(
                    "Save",
                    bankCode,
                    branchCode,
                    beforeValuesJson,
                    afterValuesJson: "{}",
                    request.ActorId,
                    request.CorrelationId),
                ct);

            await transaction.CommitAsync(ct);
            return new DeleteResult.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return new DeleteResult.DbError(ex);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────

    private static BankBranchDto ToDto(BankBranch e) =>
        new()
        {
            Id = e.Id,
            BankCode = e.BankCode,
            BranchCode = e.BranchCode,
            BankNameKana = e.BankNameKana,
            BranchNameKana = e.BranchNameKana,
            BankNameKanji = e.BankNameKanji,
            BranchNameKanji = e.BranchNameKanji,
            UpdatedAt = e.UpdatedAt
        };

    private static List<string> ValidateRequest(
        string bankCode, string branchCode,
        string bankNameKana, string branchNameKana)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(bankCode))
            errors.Add("金融機関コードは必須です。");
        if (string.IsNullOrWhiteSpace(branchCode))
            errors.Add("支店コードは必須です。");
        if (string.IsNullOrWhiteSpace(bankNameKana))
            errors.Add("金融機関名カナは必須です。");
        if (string.IsNullOrWhiteSpace(branchNameKana))
            errors.Add("支店名カナは必須です。");

        return errors;
    }

    /// <summary>
    /// 223 §6.4.1 が定める監査値JSONを組み立てる。BankNameKana、BranchNameKana以外は含めない。
    /// </summary>
    private static string BuildKanaJson(string bankNameKana, string branchNameKana) =>
        JsonSerializer.Serialize(
            new { BankNameKana = bankNameKana, BranchNameKana = branchNameKana },
            AuditJsonOptions);

    /// <summary>
    /// 223 §6.4.1 が定める TargetKey（BankCode→BranchCode の固定順）で監査エントリを組み立てる。
    /// OccurredAtは222 F-INF-003 §3.2が定めるDateTimeOffset.Now（JST）を使う。
    /// </summary>
    private static AuditLogEntry BuildAuditEntry(
        string action,
        string bankCode,
        string branchCode,
        string beforeValuesJson,
        string afterValuesJson,
        string actorId,
        string correlationId) =>
        new()
        {
            OccurredAt = DateTimeOffset.Now,
            ActorId = actorId,
            FeatureId = FeatureId,
            Action = action,
            TargetType = TargetType,
            TargetKey = JsonSerializer.Serialize(new { BankCode = bankCode, BranchCode = branchCode }, AuditJsonOptions),
            Result = "Success",
            CorrelationId = correlationId,
            BeforeValuesJson = beforeValuesJson,
            AfterValuesJson = afterValuesJson
        };
}
