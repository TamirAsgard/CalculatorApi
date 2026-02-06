namespace IO.Swagger.Configuration;

/// <summary>
/// Configuration options for JWT token generation and validation
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// JWT token issuer
    /// </summary>
    public string Issuer { get; set; } = "";
    
    /// <summary>
    /// JWT token audience
    /// </summary>
    public string Audience { get; set; } = "";
    
    /// <summary>
    /// Signing key for JWT tokens (hex-encoded)
    /// </summary>
    public string SigningKey { get; set; } = "";
    
    /// <summary>
    /// Token expiration time in minutes
    /// </summary>
    public int ExpirationMinutes { get; set; } = 30;
}