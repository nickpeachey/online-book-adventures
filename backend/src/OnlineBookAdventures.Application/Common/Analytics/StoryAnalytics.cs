namespace OnlineBookAdventures.Application.Common.Analytics;

/// <summary>
/// Holds aggregated analytics for a single story.
/// </summary>
public sealed class StoryAnalytics
{
    /// <summary>Gets the story identifier.</summary>
    public Guid StoryId { get; init; }

    /// <summary>Gets or sets the number of times users have started this story.</summary>
    public int StartCount { get; set; }

    /// <summary>Gets or sets the number of times users have completed this story.</summary>
    public int CompletionCount { get; set; }

    /// <summary>Gets or sets the total number of choices made across all readers.</summary>
    public int TotalChoicesMade { get; set; }
}
