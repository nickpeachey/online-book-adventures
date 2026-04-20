using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookAdventures.Application.Features.Stories.Commands.CreateStory;
using OnlineBookAdventures.Application.Features.Stories.Commands.DeleteStory;
using OnlineBookAdventures.Application.Features.Stories.Commands.PublishStory;
using OnlineBookAdventures.Application.Features.Stories.Commands.UpdateStory;
using OnlineBookAdventures.Application.Features.Stories.Queries.Dtos;
using OnlineBookAdventures.Application.Features.Stories.Queries.GetStory;
using OnlineBookAdventures.Application.Features.Stories.Queries.GetStoryGraph;
using OnlineBookAdventures.Application.Features.Stories.Queries.ListStories;
using OnlineBookAdventures.Application.Features.Nodes.Commands.CreateNode;
using OnlineBookAdventures.Application.Features.Nodes.Commands.UpdateNode;
using OnlineBookAdventures.Application.Features.Nodes.Commands.DeleteNode;
using OnlineBookAdventures.Application.Features.Choices.Commands.CreateChoice;
using OnlineBookAdventures.Application.Features.Choices.Commands.UpdateChoice;
using OnlineBookAdventures.Application.Features.Choices.Commands.DeleteChoice;
using OnlineBookAdventures.Application.Features.Stories.Commands.GetCoverUploadUrl;
using Microsoft.AspNetCore.RateLimiting;
using OnlineBookAdventures.Api.Configuration;
using OnlineBookAdventures.Application.Features.Stories.Commands.UploadCoverImage;

namespace OnlineBookAdventures.Api.Controllers;

/// <summary>
/// Manages story CRUD, node management, and choice management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(RateLimitingConfiguration.GeneralPolicy)]
public sealed class StoriesController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("User is not authenticated."));

    // ── Stories ──────────────────────────────────────────────────────────────

    /// <summary>Lists published stories with optional search and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ListStoriesResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListStories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ListStoriesQuery(page, pageSize, search), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a single story's metadata.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStory(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStoryQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets the full node-and-choice graph of a story.</summary>
    [HttpGet("{id:guid}/graph")]
    [ProducesResponseType(typeof(StoryGraphDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStoryGraph(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStoryGraphQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Creates a new story.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStory(
        [FromBody] CreateStoryRequest request,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(
            new CreateStoryCommand(CurrentUserId, request.Title, request.Description),
            cancellationToken);
        return CreatedAtAction(nameof(GetStory), new { id }, id);
    }

    /// <summary>Updates a story's title and description.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStory(
        Guid id,
        [FromBody] UpdateStoryRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateStoryCommand(id, CurrentUserId, request.Title, request.Description), cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a story.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStory(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteStoryCommand(id, CurrentUserId), cancellationToken);
        return NoContent();
    }

    /// <summary>Publishes or unpublishes a story.</summary>
    [HttpPatch("{id:guid}/publish")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PublishStory(
        Guid id,
        [FromBody] PublishStoryRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new PublishStoryCommand(id, CurrentUserId, request.Publish), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Uploads a cover image for a story directly from the server.
    /// </summary>
    /// <param name="id">The story identifier.</param>
    /// <param name="file">The image file to upload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The public URL of the uploaded cover image.</returns>
    [HttpPost("{id:guid}/cover-image")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadCoverImage(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        using var stream = file.OpenReadStream();
        var url = await mediator.Send(
            new UploadCoverImageCommand(id, CurrentUserId, stream, file.ContentType, file.FileName),
            cancellationToken);
        return Ok(url);
    }

    /// <summary>
    /// Generates a pre-signed URL for direct client-side cover image upload.
    /// </summary>
    /// <param name="id">The story identifier.</param>
    /// <param name="request">The upload URL request parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The pre-signed upload URL and resulting public URL.</returns>
    [HttpPost("{id:guid}/cover-upload-url")]
    [Authorize]
    [ProducesResponseType(typeof(CoverUploadUrlResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCoverUploadUrl(
        Guid id,
        [FromBody] CoverUploadUrlRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetCoverUploadUrlCommand(id, CurrentUserId, request.ContentType, request.FileExtension),
            cancellationToken);
        return Ok(result);
    }

    // ── Nodes ─────────────────────────────────────────────────────────────────

    /// <summary>Adds a new node to a story.</summary>
    [HttpPost("{storyId:guid}/nodes")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateNode(
        Guid storyId,
        [FromBody] CreateNodeRequest request,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(
            new CreateNodeCommand(storyId, CurrentUserId, request.Title, request.Content,
                request.IsStart, request.IsEnd, request.PositionX, request.PositionY),
            cancellationToken);
        return CreatedAtAction(nameof(GetStoryGraph), new { id = storyId }, id);
    }

    /// <summary>Updates a story node.</summary>
    [HttpPut("{storyId:guid}/nodes/{nodeId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateNode(
        Guid storyId,
        Guid nodeId,
        [FromBody] UpdateNodeRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new UpdateNodeCommand(nodeId, CurrentUserId, request.Title, request.Content,
                request.IsStart, request.IsEnd, request.PositionX, request.PositionY),
            cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a story node.</summary>
    [HttpDelete("{storyId:guid}/nodes/{nodeId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteNode(Guid storyId, Guid nodeId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteNodeCommand(nodeId, CurrentUserId), cancellationToken);
        return NoContent();
    }

    // ── Choices ───────────────────────────────────────────────────────────────

    /// <summary>Creates a choice connecting two nodes.</summary>
    [HttpPost("{storyId:guid}/choices")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateChoice(
        Guid storyId,
        [FromBody] CreateChoiceRequest request,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(
            new CreateChoiceCommand(request.FromNodeId, request.ToNodeId, request.Label, request.Order, CurrentUserId),
            cancellationToken);
        return CreatedAtAction(nameof(GetStoryGraph), new { id = storyId }, id);
    }

    /// <summary>Updates a choice's label and order.</summary>
    [HttpPut("{storyId:guid}/choices/{choiceId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateChoice(
        Guid storyId,
        Guid choiceId,
        [FromBody] UpdateChoiceRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateChoiceCommand(choiceId, CurrentUserId, request.Label, request.Order), cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a choice.</summary>
    [HttpDelete("{storyId:guid}/choices/{choiceId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteChoice(Guid storyId, Guid choiceId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteChoiceCommand(choiceId, CurrentUserId), cancellationToken);
        return NoContent();
    }
}

// ── Request records ───────────────────────────────────────────────────────────

/// <summary>Request body for creating a story.</summary>
public record CreateStoryRequest(string Title, string Description);

/// <summary>Request body for updating a story.</summary>
public record UpdateStoryRequest(string Title, string Description);

/// <summary>Request body for publishing a story.</summary>
public record PublishStoryRequest(bool Publish);

/// <summary>Request body for creating a node.</summary>
public record CreateNodeRequest(string Title, string Content, bool IsStart = false, bool IsEnd = false, double PositionX = 0, double PositionY = 0);

/// <summary>Request body for updating a node.</summary>
public record UpdateNodeRequest(string Title, string Content, bool IsStart, bool IsEnd, double PositionX, double PositionY);

/// <summary>Request body for creating a choice.</summary>
public record CreateChoiceRequest(Guid FromNodeId, Guid ToNodeId, string Label, int Order);

/// <summary>Request body for updating a choice.</summary>
public record UpdateChoiceRequest(string Label, int Order);

/// <summary>
/// Request body for generating a pre-signed cover image upload URL.
/// </summary>
/// <param name="ContentType">The MIME type of the image (e.g. image/jpeg).</param>
/// <param name="FileExtension">The file extension without the dot (e.g. jpg).</param>
public record CoverUploadUrlRequest(string ContentType, string FileExtension);
