namespace OnlineBookAdventures.Application.Events;

/// <summary>
/// Published when a reader makes a choice, advancing to the next node.
/// </summary>
/// <param name="UserId">The identifier of the reading user.</param>
/// <param name="StoryId">The identifier of the story.</param>
/// <param name="ChoiceId">The identifier of the choice that was made.</param>
/// <param name="FromNodeId">The node the reader was on.</param>
/// <param name="ToNodeId">The node the reader advanced to.</param>
/// <param name="OccurredAt">The UTC timestamp when the event occurred.</param>
public record ChoiceMadeEvent(Guid UserId, Guid StoryId, Guid ChoiceId, Guid FromNodeId, Guid ToNodeId, DateTimeOffset OccurredAt);
