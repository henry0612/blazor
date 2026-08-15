using System.Diagnostics;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Domain.Enums;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Infrastructure.Services;

namespace UnifiedAccount.Application.Tests.Services;

public class AuthenticationServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _sut = new AuthenticationService(_db, NullLogger<AuthenticationService>.Instance);
    }

    private async Task SeedUserAsync(string userId, string password, bool isActive = true)
    {
        _db.Users.Add(new User
        {
            UserId = userId,
            PasswordHash = PasswordHasher.HashPassword(password),
            UserName = "テストユーザー",
            Role = UserRole.Operator,
            IsActive = isActive
        });
        await _db.SaveChangesAsync();
    }

    /// <summary>このテストでは 正しい認証情報を渡したとき、ログインに成功する。</summary>
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        await SeedUserAsync("user01", "p@ssw0rd");

        var result = await _sut.LoginAsync("user01", "p@ssw0rd");

        result.IsSuccess.Should().BeTrue();
        result.UserId.Should().Be("user01");
        result.Role.Should().Be(UserRole.Operator);
        result.ErrorMessage.Should().BeNull();
    }

    /// <summary>このテストでは 誤ったパスワードを渡したとき、認証に失敗する。</summary>
    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
    {
        await SeedUserAsync("user01", "correct");

        var result = await _sut.LoginAsync("user01", "wrong");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    /// <summary>このテストでは 存在しないユーザーを指定したとき、認証に失敗する。</summary>
    [Fact]
    public async Task LoginAsync_WithNonexistentUser_ReturnsFailure()
    {
        var result = await _sut.LoginAsync("nobody", "any");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    /// <summary>このテストでは 無効なユーザーを指定したとき、認証に失敗する。</summary>
    [Fact]
    public async Task LoginAsync_WithInactiveUser_ReturnsFailure()
    {
        await SeedUserAsync("user01", "p@ssw0rd", isActive: false);

        var result = await _sut.LoginAsync("user01", "p@ssw0rd");

        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>このテストでは ログアウト処理を実行したとき、エラーなく完了する。</summary>
    [Fact]
    public async Task LogoutAsync_ShouldCompleteWithoutError()
    {
        var act = async () => await _sut.LogoutAsync();
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// このテストでは、認証成功時は UserId・UserName・Role が非 null かつ ErrorMessage が null になり、
    /// 認証失敗時は UserId・UserName・Role が null かつ ErrorMessage が非 null になることを検証する
    /// （222 F-INF-001 §3.1 LoginResult 定義）。
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithValidAndInvalidCredentials_FollowsLoginResultNullContract()
    {
        await SeedUserAsync("user01", "p@ssw0rd");

        var success = await _sut.LoginAsync("user01", "p@ssw0rd");
        var failure = await _sut.LoginAsync("user01", "wrong");

        success.IsSuccess.Should().BeTrue();
        success.UserId.Should().NotBeNull();
        success.UserName.Should().NotBeNull();
        success.Role.Should().NotBeNull();
        success.ErrorMessage.Should().BeNull();

        failure.IsSuccess.Should().BeFalse();
        failure.UserId.Should().BeNull();
        failure.UserName.Should().BeNull();
        failure.Role.Should().BeNull();
        failure.ErrorMessage.Should().NotBeNull();
    }

    /// <summary>
    /// このテストでは、存在しないユーザーを指定した場合とパスワードを誤った場合とで、
    /// ErrorMessage が完全に同一の固定文言になることを検証する（利用者列挙の防止、222 F-INF-001 §3.1）。
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithNonexistentUserAndWrongPassword_ReturnsIdenticalErrorMessage()
    {
        await SeedUserAsync("user01", "correct");

        var nonexistent = await _sut.LoginAsync("nobody", "any");
        var wrongPassword = await _sut.LoginAsync("user01", "wrong");

        nonexistent.ErrorMessage.Should().NotBeNullOrEmpty();
        nonexistent.ErrorMessage.Should().Be(wrongPassword.ErrorMessage);
    }

    /// <summary>
    /// このテストでは、ログイン失敗時にロガーへ渡されるメッセージへ平文パスワードが含まれず、
    /// かつ LoginResult.ErrorMessage にも平文パスワードが含まれないことを検証する
    /// （222 F-INF-001 §2、§3.1 パスワード非出力規定）。
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithWrongPassword_DoesNotExposePlainTextPasswordInLogsOrResult()
    {
        const string password = "P@ssw0rd-should-not-leak";
        await SeedUserAsync("user01", "correct");
        var fakeLogger = new FakeLogger<AuthenticationService>();
        var service = new AuthenticationService(_db, fakeLogger);

        var result = await service.LoginAsync("user01", password);

        fakeLogger.Entries.Should().NotContain(entry => entry.Contains(password));
        result.ErrorMessage.Should().NotContain(password);
    }

    /// <summary>
    /// このテストでは、存在しないユーザーを指定した場合でもダミーハッシュによる検証を実行し、
    /// 応答時間からユーザーの存否を判別できないことを検証する（222 F-INF-001 §3.1 手順2、§7）。
    /// 同一環境でPBKDF2の1回分の検証時間を実測して基準とし、その半分を下回らないことを確認する。
    /// 検証を省略した実装ではDB参照だけで応答するため、基準を大きく下回って失敗する。
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithNonexistentUser_ExecutesDummyHashVerification()
    {
        await SeedUserAsync("user01", "correct");
        var storedHash = PasswordHasher.HashPassword("correct");

        // 初回実行のJITウォームアップや一時的な負荷で基準が過大にならないよう、2回計測の最小値を採用する。
        var baselineMilliseconds = Math.Min(
            MeasureMilliseconds(() => PasswordHasher.VerifyPassword("wrong", storedHash)),
            MeasureMilliseconds(() => PasswordHasher.VerifyPassword("wrong", storedHash)));

        var stopwatch = Stopwatch.StartNew();
        var result = await _sut.LoginAsync("nobody", "any");
        stopwatch.Stop();

        result.IsSuccess.Should().BeFalse();
        stopwatch.Elapsed.TotalMilliseconds.Should().BeGreaterThan(baselineMilliseconds * 0.5);
    }

    /// <summary>
    /// 指定した処理の実行時間をミリ秒で計測する。
    /// </summary>
    private static double MeasureMilliseconds(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    public void Dispose() => _db.Dispose();

    /// <summary>
    /// ログ出力内容を検証するための最小限のテスト用フェイクロガー。
    /// 受け取ったログメッセージを文字列化して <see cref="Entries"/> に蓄積するだけで、実際の出力は行わない。
    /// </summary>
    private sealed class FakeLogger<T> : ILogger<T>
    {
        public List<string> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(formatter(state, exception));
        }
    }
}










