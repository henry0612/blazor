using UnifiedAccount.Application.DTOs;

namespace UnifiedAccount.Application.Services;

/// <summary>
/// 操作監査ログを永続化するサービスを提供する。
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// 操作監査ログを登録する。
    /// </summary>
    /// <param name="entry">登録する監査ログ</param>
    /// <param name="ct">キャンセルトークン</param>
    Task WriteAsync(AuditLogEntry entry, CancellationToken ct = default);
}
