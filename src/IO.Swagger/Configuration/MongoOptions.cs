namespace IO.Swagger.Configuration;

/// <summary>
/// Configuration options for MongoDB connection and database settings
/// </summary>
public sealed class MongoOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Mongo";

    /// <summary>
    /// MongoDB connection string
    /// </summary>
    public string ConnectionString { get; set; } = "";
    
    /// <summary>
    /// Name of the MongoDB database
    /// </summary>
    public string DatabaseName { get; set; } = "";
    
    /// <summary>
    /// Name of the users collection in MongoDB
    /// </summary>
    public string UsersCollection { get; set; } = "users";
}