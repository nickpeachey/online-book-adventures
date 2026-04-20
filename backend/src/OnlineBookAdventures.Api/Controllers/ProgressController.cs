using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookAdventures.Application.Features.Progress.Commands.MakeChoice;
using OnlineBookAdventures.Application.Features.Progress.Commands.ResetProgress;
using OnlineBookAdventures.Application.Features.Progress.Commands.StartStory;
using OnlineBookAdventures.Application.Features.Progress.Queries.GetProgress;

namespace OnlineBookAdventures.Api.Controllers;

/// <summary>
/// Manages a reader's progress through a story.
/// </summary>
[ApiController]
[Route("api/stories/{storyId:guid}/progress")]
[Authorize]
public sealed class ProgressController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("User is not authenticated."));

    /// <summary>
    /// Gets the current reading progress for the authenticated user.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The current progress, or 404 if the user has not started the story.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProgress(Guid storyId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProgressQuery(CurrentUserId, storyId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Starts or restarts a story, setting progress to the start node.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The identifier of the starting node.</returns>
    [HttpPost("start")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartStory(Guid storyId, CancellationToken cancellationToken)
    {
        var nodeId = await mediator.Send(new StartStoryCommand(CurrentUserId, storyId), cancellationToken);
        return Ok(nodeId);
    }

    /// <summary>
    /// Advances the reader's progress by making a choice.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <param name="request">The choice to make.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The new current node and whether the story has ended.</returns>
    [HttpPost("choose")]
    [ProducesResponseType(typeof(MakeChoiceResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MakeChoice(
        Guid storyId,
        [FromBody] MakeChoiceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new MakeChoiceCommand(CurrentUserId, storyId, request.ChoiceId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Resets the reader's progress for a story, allowing them to start from the beginning.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetProgress(Guid storyId, CancellationToken cancellationToken)
    {
        await mediator.Send(new ResetProgressCommand(CurrentUserId, storyId), cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// Request body for making a choice.
/// </summary>
/// <param name="ChoiceId">The identifier of the choice to make.</param>
public record MakeChoiceRequest(Guid ChoiceId);
