using MassTransit;
using Microsoft.Extensions.Logging;
using OnlineBookAdventures.Application.Events;

namespace OnlineBookAdventures.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes <see cref="StoryRatedEvent"/> messages and logs the rating activity.
/// </summary>
public sealed class StoryRatedConsumer(ILogger<StoryRatedConsumer> logger) : IConsumer<StoryRatedEvent>
{
    /// <inheritdoc/>
    public Task Consume(ConsumeContext<StoryRatedEvent> context)
    {
        logger.LogInformation("Story {StoryId} rated {Score}/5 by user {UserId}.",
            context.Message.StoryId, context.Message.Score, context.Message.UserId);
        return Task.CompletedTask;
    }
}
