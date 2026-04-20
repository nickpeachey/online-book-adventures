using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OnlineBookAdventures.Api.Configuration;
using OnlineBookAdventures.Application.Features.AI.Commands.GenerateFullStory;
using OnlineBookAdventures.Application.Features.AI.Commands.SuggestNodeContent;

namespace OnlineBookAdventures.Api.Controllers;

/// <summary>
/// Provides AI-powered story generation and content suggestion endpoints.
/// </summary>
[ApiController]
[Route("api/ai")]
[Authorize]
[EnableRateLimiting(RateLimitingConfiguration.GeneralPolicy)]
public sealed class AiController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Generates a full CYOA story graph from a prompt.</summary>
    [HttpPost("generate-story")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateStory(
        [FromBody] GenerateStoryRequest request,
        CancellationToken cancellationToken)
    {
        var storyId = await mediator.Send(
            new GenerateFullStoryCommand(CurrentUserId, request.Prompt), cancellationToken);
        return CreatedAtAction("GetStory", "Stories", new { id = storyId }, storyId);
    }

    /// <summary>Suggests narrative content for a story node.</summary>
    [HttpPost("stories/{storyId:guid}/suggest-node")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SuggestNodeContent(
        Guid storyId,
        [FromBody] SuggestNodeRequest request,
        CancellationToken cancellationToken)
    {
        var suggestion = await mediator.Send(
            new SuggestNodeContentCommand(storyId, CurrentUserId, request.NodeTitle, request.CurrentContent ?? ""),
            cancellationToken);
        return Ok(suggestion);
    }
}

/// <summary>Request body for generating a full story.</summary>
public record GenerateStoryRequest(string Prompt);

/// <summary>Request body for suggesting node content.</summary>
public record SuggestNodeRequest(string NodeTitle, string? CurrentContent);
