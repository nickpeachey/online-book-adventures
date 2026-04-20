using MediatR;

namespace OnlineBookAdventures.Application.Features.Ratings.Commands.RateStory;

/// <summary>
/// Command to submit or update a rating for a story.
/// </summary>
/// <param name="UserId">The identifier of the rating user.</param>
/// <param name="StoryId">The identifier of the story being rated.</param>
/// <param name="Score">The rating score between 1 and 5 inclusive.</param>
public record RateStoryCommand(Guid UserId, Guid StoryId, int Score) : IRequest;
