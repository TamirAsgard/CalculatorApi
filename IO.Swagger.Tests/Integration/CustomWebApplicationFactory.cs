using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StackExchange.Redis;

namespace IO.Swagger.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory that provides database cleanup between tests
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private IMongoDatabase? _mongoDatabase;
    private IConnectionMultiplexer? _redis;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Use test configuration if needed
            config.AddJsonFile("appsettings.Test.json", optional: true);
        });

        builder.ConfigureServices(services =>
        {
            // Store references for cleanup
            var serviceProvider = services.BuildServiceProvider();
            _mongoDatabase = serviceProvider.GetService<IMongoDatabase>();
            _redis = serviceProvider.GetService<IConnectionMultiplexer>();
        });
    }

    /// <summary>
    /// Initialize before any tests run
    /// </summary>
    public async Task InitializeAsync()
    {
        // Ensure clean state before tests
        await CleanupDatabasesAsync();
    }

    /// <summary>
    /// Cleanup after all tests complete
    /// </summary>
    public async Task DisposeAsync()
    {
        await CleanupDatabasesAsync();
    }

    /// <summary>
    /// Cleans up MongoDB and Redis databases
    /// </summary>
    public async Task CleanupDatabasesAsync()
    {
        await CleanupMongoDbAsync();
        await CleanupRedisAsync();
    }

    /// <summary>
    /// Cleans up MongoDB test data
    /// </summary>
    private async Task CleanupMongoDbAsync()
    {
        if (_mongoDatabase == null)
        {
            var serviceProvider = Services.CreateScope().ServiceProvider;
            _mongoDatabase = serviceProvider.GetService<IMongoDatabase>();
        }

        if (_mongoDatabase != null)
        {
            try
            {
                // Drop all collections in the test database
                var collections = await _mongoDatabase.ListCollectionNamesAsync();
                await collections.ForEachAsync(async collectionName =>
                {
                    await _mongoDatabase.DropCollectionAsync(collectionName);
                });
            }
            catch (Exception ex)
            {
                // Log but don't fail - database might not exist yet
                Console.WriteLine($"MongoDB cleanup warning: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Cleans up Redis test data
    /// </summary>
    private async Task CleanupRedisAsync()
    {
        if (_redis == null)
        {
            var serviceProvider = Services.CreateScope().ServiceProvider;
            _redis = serviceProvider.GetService<IConnectionMultiplexer>();
        }

        if (_redis != null)
        {
            try
            {
                var endpoints = _redis.GetEndPoints();
                foreach (var endpoint in endpoints)
                {
                    var server = _redis.GetServer(endpoint);
                    
                    // Flush the database (use with caution - only for test databases!)
                    await server.FlushDatabaseAsync();
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail - Redis might not be available
                Console.WriteLine($"Redis cleanup warning: {ex.Message}");
            }
        }
    }
}
