using System.Security.Claims;
using UnifiedAccount.Application.DTOs;

namespace UnifiedAccount.Web.Components.Pages;

/// <summary>
/// ログイン成功時のClaimsを生成する（222 F-INF-001 §6 連携仕様）。
/// </summary>
internal static class LoginClaimsFactory
{
    /// <summary>
    /// 認証結果からCookie認証用の <see cref="ClaimsIdentity"/> を生成する。
    /// </summary>
    /// <param name="result">認証結果を指定する。</param>
    /// <param name="authenticationScheme">認証スキーム名を指定する。</param>
    /// <exception cref="InvalidOperationException">
    /// 認証に失敗した結果を指定した場合に送出する。
    /// F-INF-001 §6 は Claims を <c>IsSuccess = true</c> の場合だけ生成すると定めるため、
    /// 失敗結果からのClaims生成を実装上の誤りとして扱う。
    /// </exception>
    public static ClaimsIdentity CreateIdentity(LoginResult result, string authenticationScheme)
    {
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException("認証に失敗した結果からClaimsを生成できません。");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId!),
            new(ClaimTypes.Name, result.UserName!),
            new(ClaimTypes.Role, result.Role!.Value.ToString())
        };

        return new ClaimsIdentity(claims, authenticationScheme);
    }
}
