using FluentAssertions;
using UnifiedAccount.Infrastructure.Services;

namespace UnifiedAccount.Application.Tests.Services;

public class PasswordHasherTests
{
    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void HashPassword_ShouldReturnNonEmptyBase64String()
    {
        var hash = PasswordHasher.HashPassword("TestP@ssw0rd");
        hash.Should().NotBeNullOrEmpty();
        Convert.TryFromBase64String(hash, new byte[512], out _).Should().BeTrue();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void HashPassword_ShouldProduceDifferentHashesForSamePassword()
    {
        var hash1 = PasswordHasher.HashPassword("TestP@ssw0rd");
        var hash2 = PasswordHasher.HashPassword("TestP@ssw0rd");
        hash1.Should().NotBe(hash2, because: "ソルトが毎回ランダム生成されるため");
    }

    /// <summary>このテストでは 対象条件を指定したとき、成功する。</summary>
    [Fact]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        var password = "TestP@ssw0rd";
        var hash = PasswordHasher.HashPassword(password);
        PasswordHasher.VerifyPassword(password, hash).Should().BeTrue();
    }

    /// <summary>このテストでは 誤ったパスワードを渡したとき、認証に失敗する。</summary>
    [Fact]
    public void VerifyPassword_ShouldReturnFalseForWrongPassword()
    {
        var hash = PasswordHasher.HashPassword("correct");
        PasswordHasher.VerifyPassword("wrong", hash).Should().BeFalse();
    }

    /// <summary>このテストでは 対象条件を指定したとき、失敗する。</summary>
    [Fact]
    public void VerifyPassword_ShouldReturnFalseForInvalidHashFormat()
    {
        PasswordHasher.VerifyPassword("any", "not-valid-base64!!!").Should().BeFalse();
    }

    /// <summary>このテストでは 対象条件を指定したとき、失敗する。</summary>
    [Fact]
    public void VerifyPassword_ShouldReturnFalseForTruncatedHash()
    {
        var hash = PasswordHasher.HashPassword("pw");
        var truncated = hash[..10];
        PasswordHasher.VerifyPassword("pw", truncated).Should().BeFalse();
    }
}










