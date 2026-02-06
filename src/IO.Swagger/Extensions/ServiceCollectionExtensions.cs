using IO.Swagger.Infrastructure.Context;
using IO.Swagger.Infrastructure.Repositories;
using IO.Swagger.Security;
using IO.Swagger.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace IO.Swagger.Extensions;

/// <summary>
/// Extension methods for registering application services in the DI container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services, repositories, and infrastructure components
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register Serilog logger
        services.AddSingleton(Log.Logger);
        
        // Register infrastructure
        services.AddInfrastructure();
        
        // Register security services
        services.AddSecurityServices();
        
        // Register repositories
        services.AddRepositories();
        
        // Register application services
        services.AddBusinessServices();
        
        return services;
    }
    
    /// <summary>
    /// Registers infrastructure components (MongoDB context)
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    private static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<MongoContext>();
        
        return services;
    }
    
    /// <summary>
    /// Registers security-related services (password hasher, token handler)
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    private static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenHandler, TokenHandler>();
        
        return services;
    }
    
    /// <summary>
    /// Registers repository implementations
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
    
    /// <summary>
    /// Registers business service implementations
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICalculatorService, CalculatorService>();
        
        return services;
    }
}