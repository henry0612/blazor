using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Domain.Enums;
using UnifiedAccount.Infrastructure.Services;

namespace UnifiedAccount.Infrastructure.Persistence;

/// <summary>
/// 開発環境用 DB 初期化・シーダー。
/// 本番では実行しない（Program.cs の IsDevelopment() ガードで呼び出しを制限する）。
/// admin パスワードは環境変数 DEV_ADMIN_PASSWORD から取得。未設定時はシードをスキップする。
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        var context = services.GetRequiredService<AppDbContext>();

        // EF Core InMemory の場合は IsRelational() がプロバイダー競合例外を投げることがあるため try-catch で保護する
        bool isRelational;
        try
        {
            isRelational = context.Database.IsRelational();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("database providers"))
        {
            // テスト環境でのプロバイダー競合 — シードをスキップして正常終了
            logger.LogWarning("複数の DB プロバイダーが登録されているためシードをスキップします（テスト環境）。");
            return;
        }

        if (isRelational)
        {
            await context.Database.MigrateAsync();
        }

        // admin ユーザーが既存の場合はスキップ
        if (await context.Users.AnyAsync(u => u.UserId == "admin"))
        {
            return;
        }

        var rawPassword = Environment.GetEnvironmentVariable("DEV_ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(rawPassword))
        {
            logger.LogWarning("DEV_ADMIN_PASSWORD 環境変数が未設定または空のため、admin ユーザーのシードをスキップします。");
            return;
        }

        var adminUser = new User
        {
            UserId = "admin",
            PasswordHash = PasswordHasher.HashPassword(rawPassword),
            UserName = "システム管理者",
            Role = UserRole.SystemAdmin,
            IsActive = true
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        logger.LogInformation("開発用 admin ユーザーを投入しました。本番デプロイ前にパスワードを必ず変更してください。");
    }
}
