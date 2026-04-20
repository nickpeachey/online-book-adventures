using MediatR;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Analytics.Queries.GetStoryAnalytics;

/// <summary>
/// Handles the <see cref="GetStoryAnalyticsQuery"/> by reading from the analytics store.
/// </summary>
public sealed class GetStoryAnalyticsQueryHandler(IAnalyticsStore analyticsStore)
    : IRequestHandler<GetStoryAnalyticsQuery, StoryAnalyticsDto>
{
    /// <inheritdoc/>
    public Task<StoryAnalyticsDto> Handle(GetStoryAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var analytics = analyticsStore.Get(request.StoryId);

        if (analytics is null)
            return Task.FromResult(new StoryAnalyticsDto(request.StoryId, 0, 0, 0, null));

        var completionRate = analytics.StartCount > 0
            ? (double)analytics.CompletionCount / analytics.StartCount
            : (double?)null;

        return Task.FromResult(new StoryAnalyticsDto(
            analytics.StoryId,
            analytics.StartCount,
            analytics.CompletionCount,
            analytics.TotalChoicesMade,
            completionRate));
    }
}
