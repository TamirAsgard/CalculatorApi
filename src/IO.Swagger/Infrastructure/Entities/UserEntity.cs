using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IO.Swagger.Infrastructure.Entities;

/// <summary>
/// Entity representing a user in the database
/// </summary>
public class UserEntity
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Username for authentication
    /// </summary>
    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password using PBKDF2
    /// </summary>
    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// UTC timestamp when the user was created
    /// </summary>
    [BsonElement("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}