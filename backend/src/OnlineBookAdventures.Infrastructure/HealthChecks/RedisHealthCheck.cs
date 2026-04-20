using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace OnlineBookAdventures.Infrastructure.HealthChecks;

/// <summary>
/// Health check that verifies Redis connectivity.
/// </summary>
public sealed class RedisHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = redis.GetDatabase();
            await db.PingAsync().ConfigureAwait(false);
            return HealthCheckResult.Healthy("Redis is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis is unreachable.", ex);
        }
    }
}
