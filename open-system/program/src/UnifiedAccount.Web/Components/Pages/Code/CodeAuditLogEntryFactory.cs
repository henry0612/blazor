using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using UnifiedAccount.Application.DTOs;

namespace UnifiedAccount.Web.Components.Pages.Code;

/// <summary>
/// コード設定画面の監査ログを生成する。
/// </summary>
internal static class CodeAuditLogEntryFactory
{
    private const string FeatureId = "F-ONL-100";

    private const string TargetType = "CodeSetting";

    /// <summary>
    /// コード設定検索の監査ログを生成する。
    /// </summary>
    public static AuditLogEntry CreateSearchEntry(
        ClaimsPrincipal principal,
        string codeCategory,
        string? search,
        bool hasResults = true)
    {
        return CreateEntry(
            principal,
            "Search",
            codeCategory,
            search ?? "*",
            "Success",
            hasResults ? null : "NO_DATA");
    }

    public static AuditLogEntry CreateSaveEntry(
        ClaimsPrincipal principal,
        string codeCategory,
        string codeValue)
    {
        return CreateEntry(principal, "Save", codeCategory, codeValue, "Success", null);
    }

    private static AuditLogEntry CreateEntry(
        ClaimsPrincipal principal,
        string action,
        string codeCategory,
        string codeValue,
        string result,
        string? errorCode)
    {
        var actorId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new InvalidOperationException("認証済み利用者のActorIdを取得できません。");
        }

        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
        return new AuditLogEntry
        {
            OccurredAt = DateTimeOffset.UtcNow,
            ActorId = actorId,
            FeatureId = FeatureId,
            Action = action,
            TargetType = TargetType,
            TargetKey = JsonSerializer.Serialize(new
            {
                CodeCategory = codeCategory,
                CodeValue = codeValue
            }),
            Result = result,
            CorrelationId = correlationId,
            BeforeValuesJson = "{}",
            AfterValuesJson = "{}",
            ErrorCode = errorCode
        };
    }
}
