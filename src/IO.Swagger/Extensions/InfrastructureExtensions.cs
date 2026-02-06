using System;
using IO.Swagger.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StackExchange.Redis;

namespace IO.Swagger.Extensions;

/// <summary>
/// Extension methods for configuring infrastructure services (MongoDB and Redis)
/// </summary>
public static class InfrastructureExtensions
{
    /// <summary>
    /// Loads and configures MongoDB options and services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="config">Configuration</param>
    /// <returns>Configured MongoOptions</returns>
    public static MongoOptions LoadMongoOptions(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<MongoOptions>()
            .Bind(config.GetSection(MongoOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString),
                "Mongo:ConnectionString missing")
            .Validate(o => !string.IsNullOrWhiteSpace(o.DatabaseName),
                "Mongo:DatabaseName missing")
            .Validate(o => !string.IsNullOrWhiteSpace(o.UsersCollection),
                "Mongo:UsersCollection missing")
            .ValidateOnStart();
        
        var mongoOptions = config.GetSection(MongoOptions.SectionName)
                               .Get<MongoOptions>()
                           ?? throw new InvalidOperationException("Mongo configuration missing");
        
        services.AddSingleton<IMongoClient>(_ =>
            new MongoClient(mongoOptions.ConnectionString));

        services.AddSingleton(sp =>
            sp.GetRequiredService<IMongoClient>()
                .GetDatabase(mongoOptions.DatabaseName));
        
        return mongoOptions;
    }

    /// <summary>
    /// Loads and configures Redis options and services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="config">Configuration</param>
    /// <returns>Configured RedisOptions</returns>
    public static RedisOptions LoadRedisOptions(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<RedisOptions>()
            .Bind(config.GetSection(RedisOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString),
                "Redis:ConnectionString missing")
            .Validate(o => !string.IsNullOrWhiteSpace(o.InstancePrefix),
                "Redis:InstancePrefix missing")
            .ValidateOnStart();
        
        var redisOptions = config.GetSection(RedisOptions.SectionName)
                               .Get<RedisOptions>()
                           ?? throw new InvalidOperationException("Redis configuration missing");
        
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisOptions.ConnectionString));

        return redisOptions;
    }
}