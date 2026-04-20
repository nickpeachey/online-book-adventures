using MassTransit;
using Microsoft.Extensions.Logging;
using OnlineBookAdventures.Application.Events;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes <see cref="StoryCompletedEvent"/> messages and increments the completion count.
/// </summary>
public sealed class StoryCompletedConsumer(IAnalyticsStore analyticsStore, ILogger<StoryCompletedConsumer> logger)
    : IConsumer<StoryCompletedEvent>
{
    /// <inheritdoc/>
    public Task Consume(ConsumeContext<StoryCompletedEvent> context)
    {
        var analytics = analyticsStore.GetOrCreate(context.Message.StoryId);
        analytics.CompletionCount++;
        logger.LogInformation("Story {StoryId} completed by user {UserId}. Total completions: {Count}",
            context.Message.StoryId, context.Message.UserId, analytics.CompletionCount);
        return Task.CompletedTask;
    }
}
