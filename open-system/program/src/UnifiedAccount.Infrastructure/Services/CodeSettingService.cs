using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Application.Services;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Infrastructure.Services;

public class CodeSettingService : ICodeSettingService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService? _auditLogService;
    private readonly CodeLookupCache? _codeLookupCache;

    public CodeSettingService(
        AppDbContext context,
        IAuditLogService? auditLogService = null,
        CodeLookupCache? codeLookupCache = null)
    {
        _context = context;
        _auditLogService = auditLogService;
        _codeLookupCache = codeLookupCache;
    }

    public async Task<PagedResult<CodeSettingDto>> SearchAsync(string codeCategory, string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.CodeSettings
            .AsNoTracking()
            .Where(x => x.CodeCategory == codeCategory);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.CodeValue.Contains(term) || x.DisplayText.Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CodeValue)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CodeSettingDto
            {
                Id = x.Id,
                CodeCategory = x.CodeCategory,
                CodeValue = x.CodeValue,
                DisplayText = x.DisplayText,
                DisplayOrder = x.DisplayOrder,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<CodeSettingDto>(items, totalCount, page, pageSize);
    }

    public async Task<CodeSettingDto?> GetCategoryAsync(string categoryCode, CancellationToken ct = default)
    {
        var entity = await _context.CodeSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CodeCategory == "00" && x.CodeValue == categoryCode, ct);

        return entity is null
            ? null
            : new CodeSettingDto
            {
                Id = entity.Id,
                CodeCategory = entity.CodeCategory,
                CodeValue = entity.CodeValue,
                DisplayText = entity.DisplayText,
                DisplayOrder = entity.DisplayOrder,
                UpdatedAt = entity.UpdatedAt
            };
    }

    public async Task<CodeSettingDto> SaveAsync(CodeSettingEditRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.CodeCategory) || string.IsNullOrWhiteSpace(request.CodeValue))
        {
            throw new ArgumentException("CodeCategory と CodeValue は必須です。", nameof(request));
        }

        var entity = request.Id.HasValue
            ? await _context.CodeSettings.FirstOrDefaultAsync(x => x.Id == request.Id.Value, ct)
            : null;

        if (entity is null)
        {
            if (request.OriginalUpdatedAt.HasValue)
            {
                throw new ArgumentException("新規行の OriginalUpdatedAt は null である必要があります。", nameof(request));
            }

            entity = new CodeSetting
            {
                CodeCategory = request.CodeCategory,
                CodeValue = request.CodeValue,
                DisplayText = request.DisplayText,
                DisplayOrder = request.DisplayOrder
            };
            _context.CodeSettings.Add(entity);
        }
        else
        {
            if (!request.OriginalUpdatedAt.HasValue)
            {
                throw new ArgumentException("既存行の OriginalUpdatedAt は必須です。", nameof(request));
            }

            var entry = _context.Entry(entity);
            entry.Property(item => item.UpdatedAt).OriginalValue = request.OriginalUpdatedAt.Value;
            entity.CodeCategory = request.CodeCategory;
            entity.CodeValue = request.CodeValue;
            entity.DisplayText = request.DisplayText;
            entity.DisplayOrder = request.DisplayOrder;
        }

        await _context.SaveChangesAsync(ct);
        // マスタ更新後は参照系キャッシュを破棄し、次回参照で再読込させる。
        _codeLookupCache?.Clear();

        return new CodeSettingDto
        {
            Id = entity.Id,
            CodeCategory = entity.CodeCategory,
            CodeValue = entity.CodeValue,
            DisplayText = entity.DisplayText,
            DisplayOrder = entity.DisplayOrder,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public async Task SaveBatchAsync(
        IReadOnlyList<CodeSettingEditRequest> saveRequests,
        IReadOnlyList<CodeSettingDeleteRequest> deleteRequests,
        CancellationToken ct = default,
        IReadOnlyList<AuditLogEntry>? auditEntries = null)
    {
        ValidateSaveRequests(saveRequests);
        ValidateDeleteRequests(deleteRequests);
        if (auditEntries is { Count: > 0 } && _auditLogService is null)
        {
            throw new InvalidOperationException("監査ログサービスが登録されていません。");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var deleteIds = deleteRequests.Select(request => request.Id).ToArray();
            var deleteEntities = await _context.CodeSettings
                .Where(entity => deleteIds.Contains(entity.Id))
                .ToListAsync(ct);

            foreach (var request in deleteRequests)
            {
                var entity = deleteEntities.SingleOrDefault(item => item.Id == request.Id);
                if (entity is null)
                {
                    continue;
                }

                var entry = _context.Entry(entity);
                entry.Property(item => item.UpdatedAt).OriginalValue = request.OriginalUpdatedAt;
                _context.CodeSettings.Remove(entity);
            }

            await _context.SaveChangesAsync(ct);

            var updateIds = saveRequests
                .Where(request => request.Id.HasValue)
                .Select(request => request.Id!.Value)
                .ToArray();
            var updateEntities = await _context.CodeSettings
                .Where(entity => updateIds.Contains(entity.Id))
                .ToListAsync(ct);

            foreach (var request in saveRequests)
            {
                if (request.Id.HasValue)
                {
                    var entity = updateEntities.SingleOrDefault(item => item.Id == request.Id.Value)
                        ?? throw new DbUpdateConcurrencyException("更新対象のコード設定が存在しません。");

                    var entry = _context.Entry(entity);
                    entry.Property(item => item.UpdatedAt).OriginalValue = request.OriginalUpdatedAt!.Value;
                    if (!string.Equals(entity.CodeCategory, request.CodeCategory, StringComparison.Ordinal))
                    {
                        throw new ArgumentException("更新対象の CodeCategory が一致しません。", nameof(request));
                    }

                    entity.CodeValue = request.CodeValue;
                    entity.DisplayText = request.DisplayText;
                    entity.DisplayOrder = request.DisplayOrder;
                }
                else
                {
                    _context.CodeSettings.Add(new CodeSetting
                    {
                        CodeCategory = request.CodeCategory,
                        CodeValue = request.CodeValue,
                        DisplayText = request.DisplayText,
                        DisplayOrder = request.DisplayOrder
                    });
                }
            }

            await _context.SaveChangesAsync(ct);
            if (auditEntries is not null)
            {
                foreach (var auditEntry in auditEntries)
                {
                    await _auditLogService!.WriteAsync(auditEntry, ct);
                }
            }

            await transaction.CommitAsync(ct);
            // マスタ更新後は参照系キャッシュを破棄し、次回参照で再読込させる。
            _codeLookupCache?.Clear();
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _context.CodeSettings.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return;
        }

        _context.CodeSettings.Remove(entity);
        await _context.SaveChangesAsync(ct);
        // マスタ更新後は参照系キャッシュを破棄し、次回参照で再読込させる。
        _codeLookupCache?.Clear();
    }

    private static void ValidateSaveRequests(IReadOnlyList<CodeSettingEditRequest> requests)
    {
        foreach (var request in requests)
        {
            if (string.IsNullOrWhiteSpace(request.CodeCategory)
                || string.IsNullOrWhiteSpace(request.CodeValue))
            {
                throw new ArgumentException("CodeCategory と CodeValue は必須です。", nameof(requests));
            }

            if (request.Id.HasValue && !request.OriginalUpdatedAt.HasValue)
            {
                throw new ArgumentException("既存行の OriginalUpdatedAt は必須です。", nameof(requests));
            }

            if (!request.Id.HasValue && request.OriginalUpdatedAt.HasValue)
            {
                throw new ArgumentException("新規行の OriginalUpdatedAt は null である必要があります。", nameof(requests));
            }
        }
    }

    private static void ValidateDeleteRequests(IReadOnlyList<CodeSettingDeleteRequest> requests)
    {
        if (requests.Any(request => request.OriginalUpdatedAt == default))
        {
            throw new ArgumentException("削除要求の OriginalUpdatedAt は必須です。", nameof(requests));
        }
    }

}
