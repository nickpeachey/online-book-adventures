using MediatR;

namespace OnlineBookAdventures.Application.Features.Ratings.Queries.GetStoryRating;

/// <summary>
/// Query to retrieve the aggregate rating for a story.
/// </summary>
/// <param name="StoryId">The identifier of the story.</param>
/// <param name="UserId">The optional identifier of the current user, to include their personal rating.</param>
public record GetStoryRatingQuery(Guid StoryId, Guid? UserId = null) : IRequest<StoryRatingDto>;

/// <summary>
/// Aggregate rating information for a story.
/// </summary>
/// <param name="StoryId">The story identifier.</param>
/// <param name="AverageScore">The average rating score, or <see langword="null"/> if unrated.</param>
/// <param name="TotalRatings">The total number of ratings.</param>
/// <param name="UserScore">The current user's rating score, or <see langword="null"/> if they have not rated.</param>
public record StoryRatingDto(Guid StoryId, double? AverageScore, int TotalRatings, int? UserScore);
