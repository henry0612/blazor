using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;
using UnifiedAccount.Domain.Enums;
using UnifiedAccount.Infrastructure.Configuration;
using UnifiedAccount.Web.Api;
using UnifiedAccount.Web.Components;

// Serilog bootstrap
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/web-.txt", rollingInterval: RollingInterval.Day));

    // Infrastructure (EF Core, Repositories, Services)
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHttpClient();

    // Cookie認証 (F-INF-001) — タイムアウトは appsettings.json の Auth:SessionTimeoutMinutes から読み取り
    var sessionTimeoutMinutes = builder.Configuration.GetValue<int>("Auth:SessionTimeoutMinutes", 180);
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/login";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionTimeoutMinutes);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Name = "UnifiedAccount.Auth";
        });
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("CodeSettingsEditor", policy =>
            policy.RequireRole(UserRole.SystemAdmin.ToString()));

        // F-ONL-006 金額データ修正、F-ONL-007 銀行マスタ参照・更新は画面固有の認可ポリシーを持たず、
        // FallbackPolicy（認証必須）だけに従う (222 F-INF-001 §6.1、223 F-ONL-006 v1.9、F-ONL-007 v1.4)。

        // 全ページ・全 API を認証必須にする (F-INF-001)
        // Login / Error など匿名許可が必要なページは [AllowAnonymous] を個別に付与する
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });
    builder.Services.AddCascadingAuthenticationState();

    // FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // Blazor
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    //Register HttpContextAccessor
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSerilogRequestLogging();

    // REST API endpoints (6グループ, 22エンドポイント) — 認証必須
    app.MapGroup("/api/companies").MapCompanyEndpoints().WithTags("Companies").RequireAuthorization();
    app.MapGroup("/api/contracts").MapContractEndpoints().WithTags("Contracts").RequireAuthorization();
    app.MapGroup("/api/bank-branches").MapBankBranchEndpoints().WithTags("BankBranches").RequireAuthorization();
    app.MapGroup("/api/transfers").MapTransferEndpoints().WithTags("Transfers").RequireAuthorization();
    app.MapGroup("/api/calendars").MapCalendarEndpoints().WithTags("Calendars").RequireAuthorization();
    app.MapGroup("/api/zengin").MapZenginEndpoints().WithTags("Zengin").RequireAuthorization();

    // 認証エンドポイント (ログアウト)
    app.MapAuthEndpoints();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    // 開発環境専用: 初期ユーザー seed と DB マイグレーション
    // 本番・検証環境では実行しない。DB 更新は配備手順または明示タスクで行うこと
    if (app.Environment.IsDevelopment())
    {
        try
        {
            using var scope = app.Services.CreateScope();
            await UnifiedAccount.Infrastructure.Persistence.DbInitializer.SeedAsync(
                scope.ServiceProvider, app.Logger);
        }
        catch (Exception seedEx)
        {
            Log.Warning(seedEx, "シーダー実行中にエラーが発生しました。起動を続行します。");
        }
    }

    Log.Information("統一口座振替Webアプリ起動");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Webアプリ異常終了");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program { }
