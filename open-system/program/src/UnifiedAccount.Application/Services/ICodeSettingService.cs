using UnifiedAccount.Application.DTOs;

namespace UnifiedAccount.Application.Services;

public interface ICodeSettingService
{
    Task<PagedResult<CodeSettingDto>> SearchAsync(string codeCategory, string? search, int page, int pageSize, CancellationToken ct = default);
    Task<CodeSettingDto?> GetCategoryAsync(string categoryCode, CancellationToken ct = default);
    Task<CodeSettingDto> SaveAsync(CodeSettingEditRequest request, CancellationToken ct = default);
    Task SaveBatchAsync(
        IReadOnlyList<CodeSettingEditRequest> saveRequests,
        IReadOnlyList<CodeSettingDeleteRequest> deleteRequests,
        CancellationToken ct = default,
        IReadOnlyList<AuditLogEntry>? auditEntries = null);
    Task DeleteAsync(long id, CancellationToken ct = default);
}
