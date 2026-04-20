using MediatR;

namespace OnlineBookAdventures.Application.Features.Progress.Queries.GetProgress;

/// <summary>
/// Query to retrieve a user's current progress in a story.
/// </summary>
/// <param name="UserId">The identifier of the user.</param>
/// <param name="StoryId">The identifier of the story.</param>
public record GetProgressQuery(Guid UserId, Guid StoryId) : IRequest<ProgressDto?>;

/// <summary>
/// Represents a user's reading progress in a story.
/// </summary>
/// <param name="ProgressId">The progress record identifier.</param>
/// <param name="UserId">The user identifier.</param>
/// <param name="StoryId">The story identifier.</param>
/// <param name="CurrentNodeId">The identifier of the node the reader is currently on.</param>
/// <param name="IsCompleted">Whether the reader has reached an end node.</param>
/// <param name="UpdatedAt">When this progress was last updated.</param>
public record ProgressDto(Guid ProgressId, Guid UserId, Guid StoryId, Guid CurrentNodeId, bool IsCompleted, DateTimeOffset UpdatedAt);
