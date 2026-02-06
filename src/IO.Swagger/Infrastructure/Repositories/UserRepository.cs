using System;
using System.Threading;
using System.Threading.Tasks;
using IO.Swagger.Infrastructure.Context;
using IO.Swagger.Infrastructure.Entities;
using MongoDB.Driver;

namespace IO.Swagger.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of the user repository
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly IMongoCollection<UserEntity> _users;

    /// <summary>
    /// Initializes a new instance of the UserRepository
    /// </summary>
    /// <param name="context">MongoDB context</param>
    public UserRepository(MongoContext context)
    {
        _users = context.Users;
    }

    /// <inheritdoc />
    public async Task<UserEntity> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _users
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
#nullable enable
    public async Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken ct = default)
#nullable restore
    {
        return await _users
            .Find(u => u.Username == username)
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
    {
        return await _users
            .Find(u => u.Username == username)
            .Limit(1)
            .AnyAsync(ct);
    }

    /// <inheritdoc />
    public async Task CreateAsync(UserEntity user, CancellationToken ct = default)
    {
        await _users.InsertOneAsync(user, cancellationToken: ct);
    }
}