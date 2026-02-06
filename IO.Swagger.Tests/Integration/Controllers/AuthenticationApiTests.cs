using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IO.Swagger.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IO.Swagger.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for AuthenticationApi
/// </summary>
public class AuthenticationApiTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthenticationApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Initialize before each test
    /// </summary>
    public Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// Cleanup after each test
    /// </summary>
    public async Task DisposeAsync()
    {
        await _factory.CleanupDatabasesAsync();
    }

    [Fact]
    public async Task Login_NewUser_ReturnsTokenResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = $"testuser_{Guid.NewGuid()}",
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.TokenType.Should().Be(LoginResponse.TokenTypeEnum.Bearer);
        result.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_ExistingUserCorrectPassword_ReturnsTokenResponse()
    {
        // Arrange
        var username = $"existinguser_{Guid.NewGuid()}";
        var password = "TestPassword123!";
        
        // First, register the user
        var registerRequest = new LoginRequest { Username = username, Password = password };
        await _client.PostAsJsonAsync("/v1/auth/login", registerRequest);

        // Act - Login with same credentials
        var loginRequest = new LoginRequest { Username = username, Password = password };
        var response = await _client.PostAsJsonAsync("/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_ExistingUserWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var username = $"userforwrongpwd_{Guid.NewGuid()}";
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";
        
        // First, register the user
        var registerRequest = new LoginRequest { Username = username, Password = correctPassword };
        await _client.PostAsJsonAsync("/v1/auth/login", registerRequest);

        // Act - Login with wrong password
        var loginRequest = new LoginRequest { Username = username, Password = wrongPassword };
        var response = await _client.PostAsJsonAsync("/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(null, "password")]
    [InlineData("", "password")]
    [InlineData("username", null)]
    [InlineData("username", "")]
    public async Task Login_InvalidCredentials_ReturnsBadRequest(string username, string password)
    {
        // Arrange
        var request = new LoginRequest { Username = username, Password = password };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_TokenReuse_ReturnsSameToken()
    {
        // Arrange
        var username = $"tokenreuseuser_{Guid.NewGuid()}";
        var password = "TestPassword123!";
        var request = new LoginRequest { Username = username, Password = password };

        // Act - First login
        var response1 = await _client.PostAsJsonAsync("/v1/auth/login", request);
        var result1 = await response1.Content.ReadFromJsonAsync<LoginResponse>();

        // Act - Second login immediately
        var response2 = await _client.PostAsJsonAsync("/v1/auth/login", request);
        var result2 = await response2.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert - Should reuse the same token
        result1!.Token.Should().Be(result2!.Token);
    }
}
