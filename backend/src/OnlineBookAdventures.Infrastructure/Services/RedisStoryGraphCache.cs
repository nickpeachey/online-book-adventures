using System.Text.Json;
using Microsoft.Extensions.Logging;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Application.Features.Stories.Queries.Dtos;
using StackExchange.Redis;

namespace OnlineBookAdventures.Infrastructure.Services;

/// <summary>
/// Implements story graph caching using Redis.
/// </summary>
internal sealed class RedisStoryGraphCache(
    IConnectionMultiplexer redis,
    ILogger<RedisStoryGraphCache> logger) : IStoryGraphCache
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);
    private static string CacheKey(Guid storyId) => $"story-graph:{storyId}";

    /// <inheritdoc/>
    public async Task<StoryGraphDto?> GetAsync(Guid storyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = redis.GetDatabase();
            var value = await db.StringGetAsync(CacheKey(storyId)).ConfigureAwait(false);

            if (value.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<StoryGraphDto>((string)value!);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache GET failed for story {StoryId}. Falling back to database.", storyId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync(Guid storyId, StoryGraphDto graph, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = redis.GetDatabase();
            var json = JsonSerializer.Serialize(graph);
            await db.StringSetAsync(CacheKey(storyId), json, CacheTtl).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache SET failed for story {StoryId}. Continuing without cache.", storyId);
        }
    }

    /// <inheritdoc/>
    public async Task InvalidateAsync(Guid storyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = redis.GetDatabase();
            await db.KeyDeleteAsync(CacheKey(storyId)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache INVALIDATE failed for story {StoryId}.", storyId);
        }
    }
}
