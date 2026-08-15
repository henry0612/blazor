using System.Security.Cryptography;

namespace UnifiedAccount.Infrastructure.Services;

/// <summary>
/// PBKDF2 (SHA-256) によるパスワードハッシュ/検証ユーティリティ。
/// フォーマット: Base64(salt[16] + hash[32]) = 48バイト → 64文字
/// OWASP 推奨イテレーション数 600,000 を使用。
/// </summary>
public static class PasswordHasher
{
    internal const int SaltSize = 16;
    internal const int HashSize = 32;
    private const int Iterations = 600_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// パスワードをハッシュ化して返す。
    /// </summary>
    /// <param name="password">パスワードを指定する。</param>
    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        byte[] combined = new byte[SaltSize + HashSize];
        salt.CopyTo(combined, 0);
        hash.CopyTo(combined, SaltSize);
        return Convert.ToBase64String(combined);
    }

    /// <summary>
    /// パスワードとハッシュ文字列を検証する。
    /// タイミング攻撃対策のため <see cref="CryptographicOperations.FixedTimeEquals"/> を使用。
    /// </summary>
    /// <param name="password">パスワードを指定する。</param>
    /// <param name="hashedPassword">ハッシュ化済みパスワードを指定する。</param>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        byte[] combined;
        try
        {
            combined = Convert.FromBase64String(hashedPassword);
        }
        catch (FormatException)
        {
            return false;
        }

        if (combined.Length != SaltSize + HashSize)
        {
            return false;
        }

        byte[] salt = combined[..SaltSize];
        byte[] expectedHash = combined[SaltSize..];
        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}




