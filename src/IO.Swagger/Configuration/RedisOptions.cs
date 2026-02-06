namespace IO.Swagger.Configuration;

/// <summary>
/// Configuration options for Redis connection and caching
/// </summary>
public sealed class RedisOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Redis";

    /// <summary>
    /// Redis connection string
    /// </summary>
    public string ConnectionString { get; set; } = "";
    
    /// <summary>
    /// Prefix for Redis keys to avoid collisions
    /// </summary>
    public string InstancePrefix { get; set; } = "";
}
