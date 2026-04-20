using OnlineBookAdventures.Application.Common.Analytics;

namespace OnlineBookAdventures.Application.Common.Interfaces;

/// <summary>
/// Stores and retrieves aggregated story analytics.
/// </summary>
public interface IAnalyticsStore
{
    /// <summary>
    /// Gets or creates analytics for the specified story.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <returns>The analytics record for the story.</returns>
    StoryAnalytics GetOrCreate(Guid storyId);

    /// <summary>
    /// Retrieves analytics for a story, or <see langword="null"/> if none exist.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <returns>The analytics record, or <see langword="null"/>.</returns>
    StoryAnalytics? Get(Guid storyId);
}
