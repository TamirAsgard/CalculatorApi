using System.Threading;
using System.Threading.Tasks;
using IO.Swagger.Configuration;
using IO.Swagger.Infrastructure.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace IO.Swagger.Infrastructure.Context;

/// <summary>
/// MongoDB database context for accessing collections
/// </summary>
public class MongoContext
{
    /// <summary>
    /// MongoDB database instance
    /// </summary>
    public IMongoDatabase Database { get; }
    
    /// <summary>
    /// Users collection
    /// </summary>
    public IMongoCollection<UserEntity> Users { get; }
    
    /// <summary>
    /// Initializes a new instance of the MongoContext
    /// </summary>
    /// <param name="db">MongoDB database instance</param>
    /// <param name="mongoOptions">MongoDB configuration options</param>
    public MongoContext(IMongoDatabase db, IOptions<MongoOptions> mongoOptions)
    {
        Database = db;

        var usersCollection = mongoOptions.Value.UsersCollection;
        Users = db.GetCollection<UserEntity>(usersCollection);
    }
    
    /// <summary>
    /// Ensures Mongo database is reachable, collections exist,
    /// and required indexes are created.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    public async Task EnsureDatabaseCreatedAsync(CancellationToken ct = default)
    {
        // 1. Verify connection (ping)
        var command = new BsonDocument("ping", 1);
        await Database.RunCommandAsync<BsonDocument>(command, cancellationToken: ct);

        // 2. Ensure Users collection exists
        var collections = await Database
            .ListCollectionNames()
            .ToListAsync(ct);

        if (!collections.Contains(Users.CollectionNamespace.CollectionName))
        {
            await Database.CreateCollectionAsync(
                Users.CollectionNamespace.CollectionName,
                cancellationToken: ct);
        }

        // 3. Ensure unique index on Username
        var indexModel = new CreateIndexModel<UserEntity>(
            Builders<UserEntity>.IndexKeys.Ascending(u => u.Username),
            new CreateIndexOptions
            {
                Unique = true,
                Name = "uniq_username"
            });

        await Users.Indexes.CreateOneAsync(indexModel, cancellationToken: ct);
    }
}