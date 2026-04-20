using MediatR;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.CreateStory;

/// <summary>
/// Command to create a new story.
/// </summary>
/// <param name="AuthorId">The identifier of the authoring user.</param>
/// <param name="Title">The story title.</param>
/// <param name="Description">The story description.</param>
public record CreateStoryCommand(Guid AuthorId, string Title, string Description) : IRequest<Guid>;
