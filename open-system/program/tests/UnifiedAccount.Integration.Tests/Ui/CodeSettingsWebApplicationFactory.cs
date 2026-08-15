using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Integration.Tests.Helpers;

namespace UnifiedAccount.Integration.Tests.Ui;

public sealed class CodeSettingsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            // テストを実行ユーザーのData Protectionキーリングから分離し、
            // Antiforgeryトークンをテストプロセス内だけで生成する。
            services.AddDataProtection()
                .UseEphemeralDataProtectionProvider();

            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(IDbContextFactory<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));

            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseInMemoryDatabase($"CodeSettingsUiTests_{Guid.NewGuid():N}"));
            services.AddScoped<AppDbContext>(sp =>
                sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = TestAuthHandler.SchemeName;
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName, _ => { });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        SeedTestData(host);

        return host;
    }

    /// <summary>
    /// UIテスト用のデータを投入する。
    /// テストデータはテストソース内で完結させる。業務マスタの初期データ
    /// (184コード定義書 2.13 / db/seed/CodeSettings.sql) とは独立に定義する。
    /// </summary>
    private static void SeedTestData(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var context = contextFactory.CreateDbContext();

        context.CodeSettings.AddRange(
            new CodeSetting { CodeCategory = "00", CodeValue = "BANK", DisplayText = "銀行", DisplayOrder = 10 },
            new CodeSetting { CodeCategory = "00", CodeValue = "COMPANY", DisplayText = "会社", DisplayOrder = 20 },
            new CodeSetting { CodeCategory = "BANK", CodeValue = "0001", DisplayText = "みずほ", DisplayOrder = 1 },
            new CodeSetting { CodeCategory = "BANK", CodeValue = "0005", DisplayText = "三菱", DisplayOrder = 2 });

        context.SaveChanges();
    }
}
