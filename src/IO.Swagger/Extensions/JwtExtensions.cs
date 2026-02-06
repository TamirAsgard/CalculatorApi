using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using IO.Swagger.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace IO.Swagger.Extensions;

/// <summary>
/// Extension methods for configuring JWT authentication
/// </summary>
public static class JwtExtensions
{
    /// <summary>
    /// Loads and configures JWT authentication options and services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="config">Configuration</param>
    /// <returns>Configured JwtOptions</returns>
    public static JwtOptions LoadJwtOptions(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<JwtOptions>()
            .Bind(config.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer missing")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience missing")
            .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey) && o.SigningKey.Length >= 32,
                "Jwt:SigningKey must be at least 32 characters")
            .Validate(o => o.ExpirationMinutes >= 1 && o.ExpirationMinutes <= 1440,
                "Jwt:ExpirationMinutes should be minutes (1..1440)")
            .ValidateOnStart();
        
        var jwt = config.GetSection(JwtOptions.SectionName)
                      .Get<JwtOptions>()
                  ?? throw new InvalidOperationException("Jwt configuration section is missing");
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromHexString(jwt.SigningKey)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(10)
                };
                
                // Redis Token Check - validates token exists in cache
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async ctx =>
                    {
                        var jti = ctx.Principal?
                            .FindFirst(JwtRegisteredClaimNames.Jti)?
                            .Value;

                        if (string.IsNullOrWhiteSpace(jti))
                        {
                            ctx.Fail("Missing jti");
                            return;
                        }

                        var redis = ctx.HttpContext.RequestServices
                            .GetRequiredService<IConnectionMultiplexer>()
                            .GetDatabase();

                        var redisOptions = ctx.HttpContext.RequestServices
                            .GetRequiredService<IOptions<RedisOptions>>()
                            .Value;

                        var key = $"{redisOptions.InstancePrefix}token:{jti}";
                        var exists = await redis.KeyExistsAsync(key);

                        if (!exists)
                        {
                            ctx.Fail("Token expired or revoked");
                        }
                    }
                };
            });

        services.AddAuthorization();

        return jwt;
    }
}