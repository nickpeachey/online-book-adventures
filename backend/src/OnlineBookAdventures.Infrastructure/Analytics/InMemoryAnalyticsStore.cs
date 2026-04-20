using System.Collections.Concurrent;
using OnlineBookAdventures.Application.Common.Analytics;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Infrastructure.Analytics;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IAnalyticsStore"/>.
/// </summary>
public sealed class InMemoryAnalyticsStore : IAnalyticsStore
{
    private readonly ConcurrentDictionary<Guid, StoryAnalytics> _store = new();

    /// <inheritdoc/>
    public StoryAnalytics GetOrCreate(Guid storyId) =>
        _store.GetOrAdd(storyId, id => new StoryAnalytics { StoryId = id });

    /// <inheritdoc/>
    public StoryAnalytics? Get(Guid storyId) =>
        _store.TryGetValue(storyId, out var analytics) ? analytics : null;
}
