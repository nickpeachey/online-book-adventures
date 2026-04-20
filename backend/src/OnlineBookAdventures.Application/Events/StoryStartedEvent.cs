namespace OnlineBookAdventures.Application.Events;

/// <summary>
/// Published when a user starts reading a story for the first time.
/// </summary>
/// <param name="UserId">The identifier of the reading user.</param>
/// <param name="StoryId">The identifier of the story that was started.</param>
/// <param name="OccurredAt">The UTC timestamp when the event occurred.</param>
public record StoryStartedEvent(Guid UserId, Guid StoryId, DateTimeOffset OccurredAt);
