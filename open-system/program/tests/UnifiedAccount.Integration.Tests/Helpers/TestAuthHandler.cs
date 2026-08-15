using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UnifiedAccount.Integration.Tests.Helpers;

/// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
/// 統合テスト用テスト認証ハンドラー。
/// 全リクエストを SystemAdmin として認証済みにする。
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers.TryGetValue("X-Test-Role", out var requestedRole)
            ? requestedRole.ToString()
            : "SystemAdmin";
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-admin"),
            new Claim(ClaimTypes.Name, "テスト管理者"),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}







