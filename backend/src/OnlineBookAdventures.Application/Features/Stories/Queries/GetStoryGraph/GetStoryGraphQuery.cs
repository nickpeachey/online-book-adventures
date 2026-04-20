using MediatR;
using OnlineBookAdventures.Application.Features.Stories.Queries.Dtos;

namespace OnlineBookAdventures.Application.Features.Stories.Queries.GetStoryGraph;

/// <summary>
/// Query to retrieve the full node-and-choice graph of a story.
/// </summary>
/// <param name="StoryId">The identifier of the story.</param>
public record GetStoryGraphQuery(Guid StoryId) : IRequest<StoryGraphDto>;
