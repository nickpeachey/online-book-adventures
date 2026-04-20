using MediatR;

namespace OnlineBookAdventures.Application.Features.Progress.Commands.ResetProgress;

/// <summary>
/// Command to delete a user's progress record for a story, allowing them to start over.
/// </summary>
/// <param name="UserId">The identifier of the user.</param>
/// <param name="StoryId">The identifier of the story.</param>
public record ResetProgressCommand(Guid UserId, Guid StoryId) : IRequest;
