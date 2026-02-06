using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IO.Swagger.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IO.Swagger.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for CalculationsApi
/// </summary>
public class CalculationsApiTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CalculationsApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Initialize before each test
    /// </summary>
    /// <summary>
    /// Initialize before each test
    /// </summary>
    public async Task InitializeAsync()
    {
        // Register and login to get a token
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "TestPassword123!";
        var loginRequest = new LoginRequest { Username = username, Password = password };
        
        // Register/Login (AuthService handles registration on first login attempt if configured, or I need to check API behavior)
        // Based on AuthService logic: if user not found, it registers. So POST /v1/auth/login works for registration.
        var response = await _client.PostAsJsonAsync("/v1/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        var token = result!.Token;
        
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Cleanup after each test
    /// </summary>
    public async Task DisposeAsync()
    {
        await _factory.CleanupDatabasesAsync();
    }

    [Theory]
    [InlineData("add", 5.0, 3.0, 8.0)]
    [InlineData("subtract", 10.0, 4.0, 6.0)]
    [InlineData("multiply", 6.0, 7.0, 42.0)]
    [InlineData("multiply", 6.0, 7.0, 42.0)]
    [InlineData("divide", 20.0, 4.0, 5.0)]
    [InlineData("Add", 1.0, 1.0, 2.0)]
    [InlineData("ADD", 2.0, 2.0, 4.0)]
    [InlineData("suBtract", 5.0, 2.0, 3.0)]
    public async Task PerformCalculation_ValidOperation_ReturnsCorrectResult(
        string operation, double num1, double num2, double expected)
    {
        // Arrange
        var request = new CalculationRequest { Number1 = num1, Number2 = num2 };
        _client.DefaultRequestHeaders.Add("X-Operation", operation);

        // Act
        var response = await _client.PostAsJsonAsync("/v1/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CalculationResponse>();
        result.Should().NotBeNull();
        result!.Result.Should().Be(expected);
        result.Operation.Should().Be(operation.ToLowerInvariant());
    }

    [Fact]
    public async Task PerformCalculation_DivisionByZero_ReturnsBadRequest()
    {
        // Arrange
        var request = new CalculationRequest { Number1 = 10.0, Number2 = 0.0 };
        _client.DefaultRequestHeaders.Add("X-Operation", "divide");

        // Act
        var response = await _client.PostAsJsonAsync("/v1/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PerformCalculation_MissingOperationHeader_ReturnsBadRequest()
    {
        // Arrange
        var request = new CalculationRequest { Number1 = 5.0, Number2 = 3.0 };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PerformCalculation_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var request = new CalculationRequest { Number1 = 5.0, Number2 = 3.0 };
        _client.DefaultRequestHeaders.Add("X-Operation", "invalid");

        // Act
        var response = await _client.PostAsJsonAsync("/v1/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PerformCalculation_MissingNumber1_ReturnsBadRequest()
    {
        // Arrange
        var request = new CalculationRequest { Number1 = null, Number2 = 3.0 };
        _client.DefaultRequestHeaders.Add("X-Operation", "add");

        // Act
        var response = await _client.PostAsJsonAsync("/v1/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
