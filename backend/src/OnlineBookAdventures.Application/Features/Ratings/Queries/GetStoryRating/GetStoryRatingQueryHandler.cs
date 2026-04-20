using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Ratings.Queries.GetStoryRating;

/// <summary>
/// Handles the <see cref="GetStoryRatingQuery"/>.
/// </summary>
public sealed class GetStoryRatingQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetStoryRatingQuery, StoryRatingDto>
{
    /// <inheritdoc/>
    public async Task<StoryRatingDto> Handle(GetStoryRatingQuery request, CancellationToken cancellationToken)
    {
        var ratings = await context.Ratings
            .Where(r => r.StoryId == request.StoryId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalRatings = ratings.Count;
        var averageScore = totalRatings > 0 ? ratings.Average(r => r.Score) : (double?)null;
        var userScore = request.UserId.HasValue
            ? ratings.FirstOrDefault(r => r.UserId == request.UserId.Value)?.Score
            : null;

        return new StoryRatingDto(request.StoryId, averageScore, totalRatings, userScore);
    }
}
