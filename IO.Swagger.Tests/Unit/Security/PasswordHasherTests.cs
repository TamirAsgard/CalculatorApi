using FluentAssertions;
using IO.Swagger.Security;
using Xunit;

namespace IO.Swagger.Tests.Unit.Security;

/// <summary>
/// Unit tests for PasswordHasher
/// </summary>
public class PasswordHasherTests
{
    private readonly IPasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void Hash_ValidPassword_ReturnsHashedPassword()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hashedPassword = _passwordHasher.Hash(password);

        // Assert
        hashedPassword.Should().NotBeNullOrWhiteSpace();
        hashedPassword.Should().StartWith("pbkdf2_sha256$");
        hashedPassword.Split('$').Should().HaveCount(4);
    }

    [Fact]
    public void Hash_EmptyPassword_ThrowsArgumentException()
    {
        // Arrange
        var password = "";

        // Act
        Action act = () => _passwordHasher.Hash(password);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password must not be empty.*");
    }

    [Fact]
    public void Hash_NullPassword_ThrowsArgumentException()
    {
        // Arrange
        string password = null!;

        // Act
        Action act = () => _passwordHasher.Hash(password);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Hash_SamePassword_GeneratesDifferentHashes()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hash1 = _passwordHasher.Hash(password);
        var hash2 = _passwordHasher.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2, "each hash should have a unique salt");
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var hashedPassword = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(password, hashedPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var wrongPassword = "WrongPassword456!";
        var hashedPassword = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(wrongPassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_EmptyPassword_ReturnsFalse()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var hashedPassword = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify("", hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_InvalidHashFormat_ReturnsFalse()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var invalidHash = "invalid_hash_format";

        // Act
        var result = _passwordHasher.Verify(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_TamperedHash_ReturnsFalse()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var hashedPassword = _passwordHasher.Hash(password);
        var tamperedHash = hashedPassword.Substring(0, hashedPassword.Length - 5) + "XXXXX";

        // Act
        var result = _passwordHasher.Verify(password, tamperedHash);

        // Assert
        result.Should().BeFalse();
    }
}
