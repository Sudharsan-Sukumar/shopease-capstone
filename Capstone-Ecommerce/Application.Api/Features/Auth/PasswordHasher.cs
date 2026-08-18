using System.Security.Cryptography;

namespace Application.Api.Features.Auth;

/// <summary>
/// Hand-rolled PBKDF2 hashing (not ASP.NET Core Identity's PasswordHasher)
/// to keep this Capstone's auth end-to-end hand-rolled, matching Track A's
/// A7a-Db pattern - contrast with A7c-Identity, which used the framework's
/// built-in Identity system instead.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Convert.ToHexString(salt)}.{Convert.ToHexString(hash)}";
    }

    public static bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;

        var salt = Convert.FromHexString(parts[0]);
        var expectedHash = Convert.FromHexString(parts[1]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
