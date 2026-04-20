using MediatR;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.DeleteStory;

/// <summary>
/// Command to delete a story and all its nodes and choices.
/// </summary>
/// <param name="StoryId">The identifier of the story to delete.</param>
/// <param name="RequestingUserId">The identifier of the user requesting deletion.</param>
public record DeleteStoryCommand(Guid StoryId, Guid RequestingUserId) : IRequest;
