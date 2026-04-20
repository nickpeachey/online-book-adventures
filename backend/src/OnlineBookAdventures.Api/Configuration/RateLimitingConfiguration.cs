using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace OnlineBookAdventures.Api.Configuration;

/// <summary>
/// Configures rate limiting policies for the API.
/// </summary>
public static class RateLimitingConfiguration
{
    /// <summary>The policy name applied to authentication endpoints.</summary>
    public const string AuthPolicy = "auth";

    /// <summary>The policy name applied to general read endpoints.</summary>
    public const string GeneralPolicy = "general";

    /// <summary>
    /// Adds rate limiting policies to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Strict limit for auth endpoints (5 requests per minute per IP)
            options.AddPolicy(AuthPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // General limit for read endpoints (100 requests per minute per IP)
            options.AddPolicy(GeneralPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}
