using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace IO.Swagger.Extensions;

/// <summary>
/// Extension methods for configuring Serilog logging
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures Serilog with console and file logging
    /// </summary>
    /// <param name="env">Web host environment</param>
    public static void LoadLoggingConfigurations(IWebHostEnvironment env)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                $"logs/calculator-service-{DateTime.UtcNow:yyyy-MM-dd}.txt",
                rollingInterval: RollingInterval.Infinite,
                retainedFileCountLimit: 14,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}]: {Message}{NewLine}"
            )
            .Enrich.WithProperty("Application", "IO.Swagger")
            .Enrich.WithProperty("Environment", env.EnvironmentName)
            .Enrich.FromLogContext()
            .CreateLogger();
    }
}