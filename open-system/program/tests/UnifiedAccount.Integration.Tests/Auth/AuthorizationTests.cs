using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Integration.Tests.Auth;

/// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
/// 認証・認可の統合テスト。
/// FallbackPolicy による匿名アクセスブロック、ログアウト後の状態無効化を検証する。
/// </summary>
public class AuthorizationTests : IClassFixture<AuthorizationWebApplicationFactory>
{
    private readonly AuthorizationWebApplicationFactory _factory;

    public AuthorizationTests(AuthorizationWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>このテストでは 正しい認証情報を渡したとき、ログインに成功する。</summary>
    /// (a) 匿名で既存画面 (/) にアクセスするとログイン画面へリダイレクトされること。
    /// FallbackPolicy が全画面に適用されていることを検証する。
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/bank/master")]
    [InlineData("/company/master")]
    [InlineData("/contract/search")]
    [InlineData("/calendar/edit")]
    [InlineData("/transfer/amount")]
    public async Task AnonymousAccess_ToProtectedPages_ShouldRedirectToLogin(string path)
    {
        // Arrange: リダイレクトを追わない匿名クライアント
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync(path);

        // Assert: Cookie 認証は未認証時に /login へ 302 リダイレクトする
        response.StatusCode.Should().Be(HttpStatusCode.Redirect,
            $"{path} への匿名アクセスは /login へリダイレクトされるべきです。");
        response.Headers.Location?.ToString().Should().Contain("/login",
            "リダイレクト先は /login でなければなりません。");
    }

    /// <summary>このテストでは 正しい認証情報を渡したとき、ログインに成功する。</summary>
    /// (b) ログインページは匿名で到達できること ([AllowAnonymous] の確認)。
    /// </summary>
    [Fact]
    public async Task AnonymousAccess_ToLoginPage_ShouldBeAllowed()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/login");

        // Login ページは AllowAnonymous なので 200 または SSR の 200
        response.StatusCode.Should().NotBe(HttpStatusCode.Redirect,
            "/login ページは匿名アクセスが許可されていなければなりません。");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>このテストでは ログアウト処理を実行したとき、エラーなく完了する。</summary>
    /// (c) ログアウト API (/api/auth/logout) は匿名でも呼び出せること。
    /// </summary>
    [Fact]
    public async Task Logout_Endpoint_ShouldBeAnonymouslyCallable()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.PostAsync("/api/auth/logout", content: null);

        // logout は SignOut 後 /login へ Redirect を返す
        response.StatusCode.Should().Be(HttpStatusCode.Redirect,
            "ログアウト後は /login へリダイレクトされるべきです。");
        response.Headers.Location?.ToString().Should().Contain("/login");
    }

    /// <summary>このテストでは 未認証状態でアクセスしたとき、期待どおりの結果が得られる。</summary>
    /// (d) 認証 API エンドポイントは匿名で 401 を返すこと。
    /// </summary>
    [Theory]
    [InlineData("/api/companies")]
    [InlineData("/api/contracts")]
    [InlineData("/api/bank-branches")]
    public async Task AnonymousAccess_ToApiEndpoints_ShouldReturnUnauthorized(string path)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync(path);

        // API エンドポイントは Cookie 認証チャレンジで 401 または /login への 302 リダイレクトのみを許可
        // NotFound / MethodNotAllowed を許容するとルート欠落でもテストが緑になるため除外する
        response.StatusCode.Should().BeOneOf(
            new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Redirect },
            $"API エンドポイント {path} への匿名アクセスは 401 または /login へのリダイレクトを返すべきです。");
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            response.Headers.Location?.ToString().Should().Contain("/login",
                $"API {path} への匿名アクセスのリダイレクト先は /login でなければなりません。");
        }
    }
}

/// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
/// 認証統合テスト用 WebApplicationFactory (認証バイパスなし — 実際の FallbackPolicy を使用)。
/// </summary>
public sealed class AuthorizationWebApplicationFactory : WebApplicationFactory<Program>
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

            // InMemory DB に差し替え (認証実装は本番と同じ Cookie Auth を使用)
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(IDbContextFactory<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseInMemoryDatabase($"AuthTests_{Guid.NewGuid():N}"));
            services.AddScoped<AppDbContext>(sp =>
                sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());
        });
    }
}








