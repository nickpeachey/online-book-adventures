using MediatR;

namespace OnlineBookAdventures.Application.Features.Progress.Commands.StartStory;

/// <summary>
/// Command to start or restart a story for a user, setting progress to the start node.
/// </summary>
/// <param name="UserId">The identifier of the user starting the story.</param>
/// <param name="StoryId">The identifier of the story to start.</param>
public record StartStoryCommand(Guid UserId, Guid StoryId) : IRequest<Guid>;
