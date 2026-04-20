using MediatR;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.PublishStory;

/// <summary>
/// Command to publish or unpublish a story.
/// </summary>
/// <param name="StoryId">The identifier of the story.</param>
/// <param name="RequestingUserId">The identifier of the requesting user.</param>
/// <param name="Publish"><see langword="true"/> to publish; <see langword="false"/> to unpublish.</param>
public record PublishStoryCommand(Guid StoryId, Guid RequestingUserId, bool Publish) : IRequest;
