using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IO.Swagger.Extensions;

/// <summary>
/// Extension methods for configuring global exception handling
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    /// <summary>
    /// Adds global exception handler to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<Exceptions.GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        return services;
    }

    /// <summary>
    /// Configures the application to use global exception handling middleware
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder for chaining</returns>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        
        return app;
    }
}
