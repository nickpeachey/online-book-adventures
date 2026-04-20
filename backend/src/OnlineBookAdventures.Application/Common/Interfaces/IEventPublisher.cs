namespace OnlineBookAdventures.Application.Common.Interfaces;

/// <summary>
/// Publishes domain events to the message broker.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a domain event to the message broker.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to publish.</typeparam>
    /// <param name="event">The event payload to publish.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
}
