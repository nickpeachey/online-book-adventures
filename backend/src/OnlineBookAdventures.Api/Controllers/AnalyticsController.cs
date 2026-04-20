using MediatR;
using Microsoft.AspNetCore.Mvc;
using OnlineBookAdventures.Application.Features.Analytics.Queries.GetStoryAnalytics;

namespace OnlineBookAdventures.Api.Controllers;

/// <summary>
/// Provides story analytics data.
/// </summary>
[ApiController]
[Route("api/stories/{storyId:guid}/analytics")]
public sealed class AnalyticsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets aggregated analytics for a story.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The aggregated analytics data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(StoryAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalytics(Guid storyId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStoryAnalyticsQuery(storyId), cancellationToken);
        return Ok(result);
    }
}
