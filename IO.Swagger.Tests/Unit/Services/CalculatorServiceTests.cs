using FluentAssertions;
using IO.Swagger.Common;
using IO.Swagger.Models;
using IO.Swagger.Services;
using Moq;
using Serilog;
using Xunit;

namespace IO.Swagger.Tests.Unit.Services;

/// <summary>
/// Unit tests for CalculatorService
/// </summary>
public class CalculatorServiceTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly ICalculatorService _calculatorService;

    public CalculatorServiceTests()
    {
        _loggerMock = new Mock<ILogger>();
        _calculatorService = new CalculatorService(_loggerMock.Object);
    }

    [Theory]
    [InlineData(OperationType.Add, 5.0, 3.0, 8.0)]
    [InlineData(OperationType.Add, 10.5, 2.5, 13.0)]
    public async Task CalculateAsync_Addition_ReturnsCorrectResult(OperationType operation, double num1, double num2, double expected)
    {
        // Arrange
        var request = new CalculationRequest { Number1 = num1, Number2 = num2 };

        // Act
        var result = await _calculatorService.CalculateAsync(request, operation);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(expected);
        result.Operation.Should().Be("add");
    }

    [Theory]
    [InlineData(OperationType.Subtract, 10.0, 3.0, 7.0)]
    [InlineData(OperationType.Subtract, 100.5, 50.5, 50.0)]
    public async Task CalculateAsync_Subtraction_ReturnsCorrectResult(OperationType operation, double num1, double num2, double expected)
    {
        // Arrange
        var request = new CalculationRequest { Number1 = num1, Number2 = num2 };

        // Act
        var result = await _calculatorService.CalculateAsync(request, operation);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(expected);
        result.Operation.Should().Be("subtract");
    }

    [Theory]
    [InlineData(OperationType.Multiply, 5.0, 3.0, 15.0)]
    [InlineData(OperationType.Multiply, 10.5, 2.0, 21.0)]
    public async Task CalculateAsync_Multiplication_ReturnsCorrectResult(OperationType operation, double num1, double num2, double expected)
    {
        // Arrange
        var request = new CalculationRequest { Number1 = num1, Number2 = num2 };

        // Act
        var result = await _calculatorService.CalculateAsync(request, operation);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(expected);
        result.Operation.Should().Be("multiply");
    }

    [Theory]
    [InlineData(OperationType.Divide, 10.0, 2.0, 5.0)]
    [InlineData(OperationType.Divide, 100.0, 4.0, 25.0)]
    public async Task CalculateAsync_Division_ReturnsCorrectResult(OperationType operation, double num1, double num2, double expected)
    {
        // Arrange
        var request = new CalculationRequest { Number1 = num1, Number2 = num2 };

        // Act
        var result = await _calculatorService.CalculateAsync(request, operation);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(expected);
        result.Operation.Should().Be("divide");
    }

    [Fact]
    public async Task CalculateAsync_DivisionByZero_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CalculationRequest { Number1 = 10.0, Number2 = 0.0 };

        // Act
        Func<Task> act = async () => await _calculatorService.CalculateAsync(request, OperationType.Divide);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Division by zero is not allowed.");
    }

    [Fact]
    public async Task CalculateAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        CalculationRequest request = null!;

        // Act
        Func<Task> act = async () => await _calculatorService.CalculateAsync(request, OperationType.Add);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CalculateAsync_MissingNumber1_ThrowsArgumentException()
    {
        // Arrange
        var request = new CalculationRequest { Number1 = null, Number2 = 5.0 };

        // Act
        Func<Task> act = async () => await _calculatorService.CalculateAsync(request, OperationType.Add);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Number1 is required.*");
    }

    [Fact]
    public async Task CalculateAsync_MissingNumber2_ThrowsArgumentException()
    {
        // Arrange
        var request = new CalculationRequest { Number1 = 5.0, Number2 = null };

        // Act
        Func<Task> act = async () => await _calculatorService.CalculateAsync(request, OperationType.Add);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Number2 is required.*");
    }



    [Fact]
    public async Task CalculateAsync_UnsupportedOperation_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CalculationRequest { Number1 = 5.0, Number2 = 3.0 };

        // Act
        Func<Task> act = async () => await _calculatorService.CalculateAsync(request, (OperationType)999);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Unsupported operation*");
    }

    [Fact]
    public async Task CalculateAsync_ValidCalculation_SetsTimestamp()
    {
        // Arrange
        var request = new CalculationRequest { Number1 = 5.0, Number2 = 3.0 };
        var beforeCalculation = DateTime.UtcNow;

        // Act
        var result = await _calculatorService.CalculateAsync(request, OperationType.Add);
        var afterCalculation = DateTime.UtcNow;

        // Assert
        result.Timestamp.Should().BeOnOrAfter(beforeCalculation);
        result.Timestamp.Should().BeOnOrBefore(afterCalculation);
    }
}
