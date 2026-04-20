using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookAdventures.Application.Features.Ratings.Commands.RateStory;
using OnlineBookAdventures.Application.Features.Ratings.Queries.GetStoryRating;

namespace OnlineBookAdventures.Api.Controllers;

/// <summary>
/// Manages story ratings.
/// </summary>
[ApiController]
[Route("api/stories/{storyId:guid}/ratings")]
public sealed class RatingsController(IMediator mediator) : ControllerBase
{
    private Guid? CurrentUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return value is not null ? Guid.Parse(value) : null;
        }
    }

    /// <summary>
    /// Gets the aggregate rating for a story, including the current user's rating if authenticated.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The aggregate rating information.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(StoryRatingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRating(Guid storyId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStoryRatingQuery(storyId, CurrentUserId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Submits or updates the authenticated user's rating for a story.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <param name="request">The rating to submit.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RateStory(
        Guid storyId,
        [FromBody] RateStoryRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new RateStoryCommand(CurrentUserId!.Value, storyId, request.Score), cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// Request body for submitting a rating.
/// </summary>
/// <param name="Score">The rating score between 1 and 5.</param>
public record RateStoryRequest(int Score);
