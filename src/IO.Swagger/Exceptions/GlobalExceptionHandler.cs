using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IO.Swagger.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace IO.Swagger.Exceptions;

/// <summary>
/// Global exception handler for centralized error handling across the application
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the GlobalExceptionHandler
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public GlobalExceptionHandler(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles exceptions globally and returns appropriate error responses
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.Error(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, errorType, message) = exception switch
        {
            InvalidPasswordException => (
                StatusCodes.Status401Unauthorized,
                "Invalid Credentials",
                exception.Message
            ),
            ArgumentException or ArgumentNullException => (
                StatusCodes.Status400BadRequest,
                "Invalid Input",
                exception.Message
            ),
            InvalidOperationException => (
                StatusCodes.Status400BadRequest,
                "Operation Error",
                exception.Message
            ),
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "You do not have permission to access this resource."
            ),
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Not Found",
                exception.Message
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later."
            )
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var errorResponse = new Error
        {
            _Error = errorType,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true; // Exception handled
    }
}
