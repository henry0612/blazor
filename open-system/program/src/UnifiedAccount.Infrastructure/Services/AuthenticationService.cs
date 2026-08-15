using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Application.Services;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Infrastructure.Services;

/// <summary>
/// Cookie認証用 認証サービス実装 (F-INF-001)
/// パスワード平文はログ出力・例外メッセージに含めない。
/// </summary>
public sealed class AuthenticationService : IAuthenticationService
{
    // タイミング攻撃対策: ユーザー不在時でも PBKDF2 を実行するためのダミーハッシュ
    // Base64(byte[48]{0}) = salt[16]{0} + hash[32]{0}
    private static readonly string DummyHash = Convert.ToBase64String(new byte[PasswordHasher.SaltSize + PasswordHasher.HashSize]);

    private const string InvalidCredentialsMessage = "ユーザーIDまたはパスワードが正しくありません。";

    /// <summary>
    /// データベースを保持する。
    /// </summary>
    private readonly AppDbContext _db;
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(AppDbContext db, ILogger<AuthenticationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<LoginResult> LoginAsync(string userId, string password, CancellationToken ct = default)
    {
        // 入力値は呼び出し元でバリデーション済み
        var user = await _db.Users
            .Where(u => u.UserId == userId && u.IsActive)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (user is null)
        {
            // ユーザー不在でもタイミング攻撃を防ぐためダミーハッシュで検証
            PasswordHasher.VerifyPassword(password, DummyHash);
            _logger.LogWarning("ログイン失敗: ユーザーID={UserId} が存在しないか無効", userId);
            return new LoginResult(false, ErrorMessage: InvalidCredentialsMessage);
        }

        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            _logger.LogWarning("ログイン失敗: ユーザーID={UserId} のパスワード不一致", userId);
            return new LoginResult(false, ErrorMessage: InvalidCredentialsMessage);
        }

        _logger.LogInformation("ログイン成功: ユーザーID={UserId}", userId);
        return new LoginResult(true, user.UserId, user.UserName, user.Role);
    }

    /// <inheritdoc />
    /// <remarks>Cookie 削除は Web 層の HttpContext.SignOutAsync で実施。</remarks>
    public Task LogoutAsync(CancellationToken ct = default) => Task.CompletedTask;
}
