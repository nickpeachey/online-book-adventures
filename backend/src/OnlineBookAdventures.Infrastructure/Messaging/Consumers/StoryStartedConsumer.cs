using MassTransit;
using Microsoft.Extensions.Logging;
using OnlineBookAdventures.Application.Events;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes <see cref="StoryStartedEvent"/> messages and increments the start count.
/// </summary>
public sealed class StoryStartedConsumer(IAnalyticsStore analyticsStore, ILogger<StoryStartedConsumer> logger)
    : IConsumer<StoryStartedEvent>
{
    /// <inheritdoc/>
    public Task Consume(ConsumeContext<StoryStartedEvent> context)
    {
        var analytics = analyticsStore.GetOrCreate(context.Message.StoryId);
        analytics.StartCount++;
        logger.LogInformation("Story {StoryId} started by user {UserId}. Total starts: {Count}",
            context.Message.StoryId, context.Message.UserId, analytics.StartCount);
        return Task.CompletedTask;
    }
}
