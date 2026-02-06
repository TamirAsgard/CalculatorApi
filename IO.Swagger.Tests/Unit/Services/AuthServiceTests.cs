using FluentAssertions;
using IO.Swagger.Exceptions;
using IO.Swagger.Infrastructure.Entities;
using IO.Swagger.Infrastructure.Repositories;
using IO.Swagger.Models;
using IO.Swagger.Security;
using IO.Swagger.Services;
using Moq;
using Serilog;
using Xunit;

namespace IO.Swagger.Tests.Unit.Services;

/// <summary>
/// Unit tests for AuthService
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenHandler> _tokenHandlerMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly IAuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenHandlerMock = new Mock<ITokenHandler>();
        _loggerMock = new Mock<ILogger>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenHandlerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task LoginOrRegisterAsync_NewUser_CreatesUserAndReturnsToken()
    {
        // Arrange
        var request = new LoginRequest { Username = "newuser", Password = "password123" };
        var hashedPassword = "hashed_password";
        var token = "jwt_token";
        var expiresIn = 1800;

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(request.Username, default))
            .ReturnsAsync((UserEntity?)null);

        _passwordHasherMock
            .Setup(x => x.Hash(request.Password))
            .Returns(hashedPassword);

        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), default))
            .Returns(Task.CompletedTask);

        _tokenHandlerMock
            .Setup(x => x.GetExistingTokenAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((ValueTuple<string, int>?)null);

        _tokenHandlerMock
            .Setup(x => x.CreateAccessTokenAsync(It.IsAny<Guid>(), request.Username, default))
            .ReturnsAsync((token, expiresIn));

        // Act
        var result = await _authService.LoginOrRegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(token);
        result.ExpiresIn.Should().Be(expiresIn);
        result.TokenType.Should().Be(LoginResponse.TokenTypeEnum.Bearer);

        _userRepositoryMock.Verify(x => x.CreateAsync(
            It.Is<UserEntity>(u => u.Username == request.Username && u.PasswordHash == hashedPassword),
            default), Times.Once);
    }

    [Fact]
    public async Task LoginOrRegisterAsync_ExistingUserCorrectPassword_ReturnsToken()
    {
        // Arrange
        var request = new LoginRequest { Username = "existinguser", Password = "password123" };
        var userId = Guid.NewGuid();
        var existingUser = new UserEntity
        {
            Id = userId,
            Username = request.Username,
            PasswordHash = "hashed_password"
        };
        var token = "jwt_token";
        var expiresIn = 1800;

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(request.Username, default))
            .ReturnsAsync(existingUser);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, existingUser.PasswordHash))
            .Returns(true);

        _tokenHandlerMock
            .Setup(x => x.GetExistingTokenAsync(userId, default))
            .ReturnsAsync((ValueTuple<string, int>?)null);

        _tokenHandlerMock
            .Setup(x => x.CreateAccessTokenAsync(userId, request.Username, default))
            .ReturnsAsync((token, expiresIn));

        // Act
        var result = await _authService.LoginOrRegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(token);
        result.ExpiresIn.Should().Be(expiresIn);

        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    [Fact]
    public async Task LoginOrRegisterAsync_ExistingUserWrongPassword_ThrowsInvalidPasswordException()
    {
        // Arrange
        var request = new LoginRequest { Username = "existinguser", Password = "wrongpassword" };
        var existingUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = "hashed_password"
        };

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(request.Username, default))
            .ReturnsAsync(existingUser);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, existingUser.PasswordHash))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _authService.LoginOrRegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidPasswordException>();
    }

    [Fact]
    public async Task LoginOrRegisterAsync_ExistingToken_ReusesToken()
    {
        // Arrange
        var request = new LoginRequest { Username = "existinguser", Password = "password123" };
        var userId = Guid.NewGuid();
        var existingUser = new UserEntity
        {
            Id = userId,
            Username = request.Username,
            PasswordHash = "hashed_password"
        };
        var existingToken = "existing_jwt_token";
        var expiresIn = 900;

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(request.Username, default))
            .ReturnsAsync(existingUser);

        _passwordHasherMock
            .Setup(x => x.Verify(request.Password, existingUser.PasswordHash))
            .Returns(true);

        _tokenHandlerMock
            .Setup(x => x.GetExistingTokenAsync(userId, default))
            .ReturnsAsync((existingToken, expiresIn));

        // Act
        var result = await _authService.LoginOrRegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(existingToken);
        result.ExpiresIn.Should().Be(expiresIn);

        _tokenHandlerMock.Verify(x => x.CreateAccessTokenAsync(It.IsAny<Guid>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task LoginOrRegisterAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        LoginRequest request = null!;

        // Act
        Func<Task> act = async () => await _authService.LoginOrRegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "password")]
    [InlineData("", "password")]
    [InlineData(" ", "password")]
    [InlineData("username", null)]
    [InlineData("username", "")]
    [InlineData("username", " ")]
    public async Task LoginOrRegisterAsync_InvalidCredentials_ThrowsArgumentException(string username, string password)
    {
        // Arrange
        var request = new LoginRequest { Username = username, Password = password };

        // Act
        Func<Task> act = async () => await _authService.LoginOrRegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
