using UnifiedAccount.Application.DTOs;

namespace UnifiedAccount.Application.Services;

/// <summary>
/// 認証サービスインタフェース (F-INF-001)
/// 実装: UnifiedAccount.Infrastructure.Services.AuthenticationService
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// ユーザー認証。成功時は LoginResult.IsSuccess = true を返す。
    /// </summary>
    Task<LoginResult> LoginAsync(string userId, string password, CancellationToken ct = default);

    /// <summary>
    /// ログアウト処理 (Cookie 削除は Web 層の HttpContext.SignOutAsync で実施)。
    /// </summary>
    Task LogoutAsync(CancellationToken ct = default);
}

