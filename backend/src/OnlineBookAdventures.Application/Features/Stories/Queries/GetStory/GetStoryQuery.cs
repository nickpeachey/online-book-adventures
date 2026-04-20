using MediatR;
using OnlineBookAdventures.Application.Features.Stories.Queries.Dtos;

namespace OnlineBookAdventures.Application.Features.Stories.Queries.GetStory;

/// <summary>
/// Query to retrieve a single story's metadata.
/// </summary>
/// <param name="StoryId">The identifier of the story to retrieve.</param>
public record GetStoryQuery(Guid StoryId) : IRequest<StoryDto>;
