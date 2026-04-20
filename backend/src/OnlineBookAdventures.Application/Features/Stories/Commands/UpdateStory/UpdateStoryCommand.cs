using MediatR;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.UpdateStory;

/// <summary>
/// Command to update an existing story's metadata.
/// </summary>
/// <param name="StoryId">The identifier of the story to update.</param>
/// <param name="RequestingUserId">The identifier of the user requesting the update.</param>
/// <param name="Title">The new story title.</param>
/// <param name="Description">The new story description.</param>
public record UpdateStoryCommand(Guid StoryId, Guid RequestingUserId, string Title, string Description) : IRequest;
