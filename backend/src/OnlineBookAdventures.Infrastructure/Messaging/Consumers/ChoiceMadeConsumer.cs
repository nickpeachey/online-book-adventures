using MassTransit;
using Microsoft.Extensions.Logging;
using OnlineBookAdventures.Application.Events;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes <see cref="ChoiceMadeEvent"/> messages and increments the choice count.
/// </summary>
public sealed class ChoiceMadeConsumer(IAnalyticsStore analyticsStore, ILogger<ChoiceMadeConsumer> logger)
    : IConsumer<ChoiceMadeEvent>
{
    /// <inheritdoc/>
    public Task Consume(ConsumeContext<ChoiceMadeEvent> context)
    {
        var analytics = analyticsStore.GetOrCreate(context.Message.StoryId);
        analytics.TotalChoicesMade++;
        logger.LogInformation("Choice made in story {StoryId} by user {UserId}. Total choices: {Count}",
            context.Message.StoryId, context.Message.UserId, analytics.TotalChoicesMade);
        return Task.CompletedTask;
    }
}
