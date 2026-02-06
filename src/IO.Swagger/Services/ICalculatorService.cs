using System.Threading;
using System.Threading.Tasks;
using IO.Swagger.Common;
using IO.Swagger.Models;

namespace IO.Swagger.Services;

/// <summary>
/// Service for performing mathematical calculations
/// </summary>
public interface ICalculatorService
{
    /// <summary>
    /// Performs a mathematical calculation based on the operation type
    /// </summary>
    /// <param name="request">Calculation request containing the two numbers</param>
    /// <param name="operation">Operation to perform (add, subtract, multiply, divide)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Calculation response with result and timestamp</returns>
    Task<CalculationResponse> CalculateAsync(CalculationRequest request, OperationType operation, CancellationToken ct = default);
}

