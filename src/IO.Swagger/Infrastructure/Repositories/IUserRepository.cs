using System;
using System.Threading;
using System.Threading.Tasks;
using IO.Swagger.Infrastructure.Entities;

namespace IO.Swagger.Infrastructure.Repositories;

/// <summary>
/// Repository for user data access operations
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by their unique identifier
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User entity or null if not found</returns>
    Task<UserEntity> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Retrieves a user by their username
    /// </summary>
    /// <param name="username">Username to search for</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User entity or null if not found</returns>
#nullable enable
    Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken ct = default);
#nullable restore

    /// <summary>
    /// Checks if a username already exists in the database
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if username exists, false otherwise</returns>
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Creates a new user in the database
    /// </summary>
    /// <param name="user">User entity to create</param>
    /// <param name="ct">Cancellation token</param>
    Task CreateAsync(UserEntity user, CancellationToken ct = default);
}