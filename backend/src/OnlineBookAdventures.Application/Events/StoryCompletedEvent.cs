namespace OnlineBookAdventures.Application.Events;

/// <summary>
/// Published when a reader reaches a terminal (end) node in a story.
/// </summary>
/// <param name="UserId">The identifier of the reading user.</param>
/// <param name="StoryId">The identifier of the completed story.</param>
/// <param name="EndNodeId">The identifier of the terminal node reached.</param>
/// <param name="OccurredAt">The UTC timestamp when the event occurred.</param>
public record StoryCompletedEvent(Guid UserId, Guid StoryId, Guid EndNodeId, DateTimeOffset OccurredAt);
