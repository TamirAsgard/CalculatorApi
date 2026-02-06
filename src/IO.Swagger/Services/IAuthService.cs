using System.Threading;
using System.Threading.Tasks;
using IO.Swagger.Models;

namespace IO.Swagger.Services;

/// <summary>
/// Service for handling user authentication and registration
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates an existing user or registers a new user
    /// </summary>
    /// <param name="request">Login credentials containing username and password</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Login response with access token and expiration</returns>
    Task<LoginResponse> LoginOrRegisterAsync(LoginRequest request, CancellationToken ct = default);
}
