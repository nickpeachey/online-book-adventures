namespace OnlineBookAdventures.Application.Events;

/// <summary>
/// Published when a user rates a story.
/// </summary>
/// <param name="UserId">The identifier of the rating user.</param>
/// <param name="StoryId">The identifier of the rated story.</param>
/// <param name="Score">The rating score (1–5).</param>
/// <param name="OccurredAt">The UTC timestamp when the event occurred.</param>
public record StoryRatedEvent(Guid UserId, Guid StoryId, int Score, DateTimeOffset OccurredAt);
