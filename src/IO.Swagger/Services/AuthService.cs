using System;
using System.Threading;
using System.Threading.Tasks;
using IO.Swagger.Exceptions;
using IO.Swagger.Infrastructure.Entities;
using IO.Swagger.Infrastructure.Repositories;
using IO.Swagger.Models;
using IO.Swagger.Security;
using Serilog;

namespace IO.Swagger.Services;

/// <summary>
/// Authentication service for user login and registration
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenHandler _tokenHandler;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the AuthService
    /// </summary>
    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenHandler tokenHandler,
        ILogger logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenHandler = tokenHandler;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<LoginResponse> LoginOrRegisterAsync(LoginRequest request, CancellationToken ct = default)
    {
        // Validate input parameters
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            _logger.Warning("Login attempt with empty username");
            throw new ArgumentException("Username cannot be empty.", nameof(request.Username));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.Warning("Login attempt with empty password for username: {Username}", request.Username);
            throw new ArgumentException("Password cannot be empty.", nameof(request.Password));
        }

        _logger.Information("Login/Register attempt for username: {Username}", request.Username);

        // Check if user exists in database
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, ct);

        if (existingUser == null)
        {
            // User doesn't exist - register new user
            _logger.Information("User {Username} not found, registering new user", request.Username);
            return await RegisterNewUserAsync(request.Username, request.Password, ct);
        }
        else
        {
            // User exists - verify password and handle token
            _logger.Information("User {Username} found, attempting login", request.Username);
            return await LoginExistingUserAsync(existingUser, request.Password, ct);
        }
    }

    private async Task<LoginResponse> RegisterNewUserAsync(string username, string password, CancellationToken ct)
    {
        _logger.Information("Starting registration for new user: {Username}", username);

        // Hash the password using PBKDF2
        var passwordHash = _passwordHasher.Hash(password);

        // Create new user entity
        var newUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHash,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Save user to database
        await _userRepository.CreateAsync(newUser, ct);
        _logger.Information("User {Username} created successfully with ID: {UserId}", username, newUser.Id);

        // Generate access token (TokenHandler stores it in Redis)
        var (token, expiresInSeconds) = await _tokenHandler.CreateAccessTokenAsync(
            newUser.Id,
            newUser.Username,
            ct);

        _logger.Information("Access token generated for new user {Username}, expires in {ExpiresIn} seconds", 
            username, expiresInSeconds);
        
        // Return response
        return new LoginResponse
        {
            Token = token,
            ExpiresIn = expiresInSeconds,
            TokenType = LoginResponse.TokenTypeEnum.Bearer
        };
    }

    private async Task<LoginResponse> LoginExistingUserAsync(UserEntity user, string password, CancellationToken ct)
    {
        _logger.Information("Verifying password for user: {Username}", user.Username);

        // Verify password using constant-time comparison
        if (!_passwordHasher.Verify(password, user.PasswordHash))
        {
            _logger.Warning("Invalid password attempt for user: {Username}", user.Username);
            throw new InvalidPasswordException("The provided password is incorrect.");
        }

        _logger.Information("Password verified successfully for user: {Username}", user.Username);

        // Check if there's an existing valid token in Redis
        var existingToken = await _tokenHandler.GetExistingTokenAsync(user.Id, ct);

        if (existingToken.HasValue)
        {
            // Valid token exists, return it
            _logger.Information("Returning existing token for user {Username}, expires in {ExpiresIn} seconds", 
                user.Username, existingToken.Value.ExpiresInSeconds);
            
            return new LoginResponse
            {
                Token = existingToken.Value.Token,
                ExpiresIn = existingToken.Value.ExpiresInSeconds,
                TokenType = LoginResponse.TokenTypeEnum.Bearer
            };
        }

        // No valid token - create new one
        _logger.Information("No valid token found for user {Username}, generating new token", user.Username);
        
        var (newToken, expiresInSeconds) = await _tokenHandler.CreateAccessTokenAsync(
            user.Id,
            user.Username,
            ct);

        _logger.Information("New access token generated for user {Username}, expires in {ExpiresIn} seconds", 
            user.Username, expiresInSeconds);

        return new LoginResponse
        {
            Token = newToken,
            ExpiresIn = expiresInSeconds,
            TokenType = LoginResponse.TokenTypeEnum.Bearer
        };
    }
}

