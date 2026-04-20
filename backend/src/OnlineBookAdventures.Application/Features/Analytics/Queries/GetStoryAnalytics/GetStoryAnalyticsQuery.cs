using MediatR;

namespace OnlineBookAdventures.Application.Features.Analytics.Queries.GetStoryAnalytics;

/// <summary>
/// Query to retrieve aggregated analytics for a story.
/// </summary>
/// <param name="StoryId">The identifier of the story.</param>
public record GetStoryAnalyticsQuery(Guid StoryId) : IRequest<StoryAnalyticsDto>;

/// <summary>
/// Aggregated analytics for a story.
/// </summary>
/// <param name="StoryId">The story identifier.</param>
/// <param name="StartCount">Number of times the story has been started.</param>
/// <param name="CompletionCount">Number of times the story has been completed.</param>
/// <param name="TotalChoicesMade">Total number of choices made across all readers.</param>
/// <param name="CompletionRate">The ratio of completions to starts, or <see langword="null"/> if no starts.</param>
public record StoryAnalyticsDto(Guid StoryId, int StartCount, int CompletionCount, int TotalChoicesMade, double? CompletionRate);
