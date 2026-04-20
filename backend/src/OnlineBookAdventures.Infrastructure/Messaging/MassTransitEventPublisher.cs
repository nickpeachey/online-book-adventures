using MassTransit;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Infrastructure.Messaging;

/// <summary>
/// Implements <see cref="IEventPublisher"/> using MassTransit's publish bus.
/// </summary>
internal sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        await publishEndpoint.Publish(@event, cancellationToken).ConfigureAwait(false);
    }
}
