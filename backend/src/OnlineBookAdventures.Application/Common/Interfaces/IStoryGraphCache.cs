using OnlineBookAdventures.Application.Features.Stories.Queries.Dtos;

namespace OnlineBookAdventures.Application.Common.Interfaces;

/// <summary>
/// Provides caching for story graphs to reduce database round-trips during reading.
/// </summary>
public interface IStoryGraphCache
{
    /// <summary>
    /// Retrieves a cached story graph, or <see langword="null"/> if not cached.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The cached <see cref="StoryGraphDto"/>, or <see langword="null"/> if the cache entry does not exist.</returns>
    Task<StoryGraphDto?> GetAsync(Guid storyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a story graph in the cache.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <param name="graph">The story graph to cache.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SetAsync(Guid storyId, StoryGraphDto graph, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a story graph from the cache, typically after it is modified.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task InvalidateAsync(Guid storyId, CancellationToken cancellationToken = default);
}
