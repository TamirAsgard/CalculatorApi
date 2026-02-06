using System;
using System.Threading;
using System.Threading.Tasks;
using IO.Swagger.Common;
using IO.Swagger.Models;
using Serilog;

namespace IO.Swagger.Services;

/// <summary>
/// Service for performing mathematical calculations
/// </summary>
public class CalculatorService : ICalculatorService
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the CalculatorService
    /// </summary>
    public CalculatorService(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<CalculationResponse> CalculateAsync(CalculationRequest request, OperationType operation, CancellationToken ct = default)
    {
        // Validate inputs
        if (request == null)
        {
            _logger.Warning("Calculation attempt with null request");
            throw new ArgumentNullException(nameof(request));
        }

        if (!request.Number1.HasValue)
        {
            _logger.Warning("Calculation attempt with missing Number1");
            throw new ArgumentException("Number1 is required.", nameof(request));
        }

        if (!request.Number2.HasValue)
        {
            _logger.Warning("Calculation attempt with missing Number2");
            throw new ArgumentException("Number2 is required.", nameof(request));
        }

        var num1 = request.Number1.Value;
        var num2 = request.Number2.Value;

        _logger.Information("Performing {Operation} calculation: {Num1} and {Num2}", 
            operation.ToString(), num1, num2);

        // Perform calculation with overflow and division by zero checks
        double result;
        try
        {
            result = operation switch
            {
                OperationType.Add => CheckedAdd(num1, num2),
                OperationType.Subtract => CheckedSubtract(num1, num2),
                OperationType.Multiply => CheckedMultiply(num1, num2),
                OperationType.Divide => CheckedDivide(num1, num2),
                _ => throw new InvalidOperationException($"Unsupported operation: {operation.ToString()}")
            };

            _logger.Information("Calculation successful: {Num1} {Operation} {Num2} = {Result}", 
                num1, operation.ToString(), num2, result);
        }
        catch (DivideByZeroException ex)
        {
            _logger.Warning("Division by zero attempted: {Num1} / {Num2}", num1, num2);
            throw new InvalidOperationException("Division by zero is not allowed.", ex);
        }
        catch (OverflowException ex)
        {
            _logger.Warning("Overflow occurred during {Operation}: {Num1} and {Num2}", 
                operation.ToString(), num1, num2);
            throw new InvalidOperationException("The calculation resulted in an overflow.", ex);
        }

        // Create response
        var response = new CalculationResponse
        {
            Result = result,
            Operation = operation.ToString().ToLowerInvariant(),
            Timestamp = DateTime.UtcNow
        };

        return Task.FromResult(response);
    }



    private static double CheckedAdd(double a, double b)
    {
        var result = a + b;
        if (double.IsInfinity(result) || double.IsNaN(result))
            throw new OverflowException("Addition resulted in overflow.");
        return result;
    }

    private static double CheckedSubtract(double a, double b)
    {
        var result = a - b;
        if (double.IsInfinity(result) || double.IsNaN(result))
            throw new OverflowException("Subtraction resulted in overflow.");
        return result;
    }

    private static double CheckedMultiply(double a, double b)
    {
        var result = a * b;
        if (double.IsInfinity(result) || double.IsNaN(result))
            throw new OverflowException("Multiplication resulted in overflow.");
        return result;
    }

    private static double CheckedDivide(double a, double b)
    {
        // Check for division by zero using epsilon comparison
        if (Math.Abs(b) < double.Epsilon)
            throw new DivideByZeroException("Cannot divide by zero.");

        var result = a / b;
        if (double.IsInfinity(result) || double.IsNaN(result))
            throw new OverflowException("Division resulted in overflow.");
        return result;
    }
}
