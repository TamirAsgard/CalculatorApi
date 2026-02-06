using System;
using System.Security.Cryptography;

namespace IO.Swagger.Security;

/// <summary>
/// Interface for password hashing and verification
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password using PBKDF2
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password string</returns>
    string Hash(string password);
    
    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hashed">Hashed password to compare against</param>
    /// <returns>True if password matches, false otherwise</returns>
    bool Verify(string password, string hashed);
}

/// <summary>
/// PBKDF2-based password hasher for secure password storage
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    // PBKDF2 parameters
    private const int SaltSizeBytes = 16;      // 128-bit salt
    private const int KeySizeBytes = 32;       // 256-bit derived key
    private const int Iterations = 100_000;    // adjust if needed (higher = slower, more secure)

    /// <inheritdoc />
    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password must not be empty.", nameof(password));

        // Generate random salt
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);

        // Derive key using PBKDF2 with SHA256
        var key = Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: KeySizeBytes);

        // Format: pbkdf2_sha256$iterations$saltBase64$keyBase64
        return $"pbkdf2_sha256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    /// <inheritdoc />
    public bool Verify(string password, string hashed)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        if (string.IsNullOrWhiteSpace(hashed)) return false;

        // Expected format: pbkdf2_sha256$iterations$salt$key
        var parts = hashed.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4) return false;
        if (!string.Equals(parts[0], "pbkdf2_sha256", StringComparison.Ordinal)) return false;

        if (!int.TryParse(parts[1], out int iterations) || iterations < 10_000)
            return false;

        byte[] salt;
        byte[] storedKey;

        try
        {
            salt = Convert.FromBase64String(parts[2]);
            storedKey = Convert.FromBase64String(parts[3]);
        }
        catch
        {
            return false;
        }

        // Derive key from provided password using stored salt and iterations
        var computedKey = Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: salt,
            iterations: iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: KeySizeBytes);

        // Constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(computedKey, storedKey);
    }
}
