using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookAdventures.Application.Features.Comments.Commands.AddComment;
using OnlineBookAdventures.Application.Features.Comments.Commands.DeleteComment;
using OnlineBookAdventures.Application.Features.Comments.Queries.ListComments;

namespace OnlineBookAdventures.Api.Controllers;

/// <summary>
/// Manages story comments.
/// </summary>
[ApiController]
[Route("api/stories/{storyId:guid}/comments")]
public sealed class CommentsController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("User is not authenticated."));

    /// <summary>
    /// Lists paginated comments for a story.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of comments per page.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated list of comments.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ListCommentsResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListComments(
        Guid storyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ListCommentsQuery(storyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Adds a comment to a story.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <param name="request">The comment to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The identifier of the newly created comment.</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddComment(
        Guid storyId,
        [FromBody] AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new AddCommentCommand(CurrentUserId, storyId, request.Body), cancellationToken);
        return CreatedAtAction(nameof(ListComments), new { storyId }, id);
    }

    /// <summary>
    /// Deletes a comment. Only the comment author may delete their own comment.
    /// </summary>
    /// <param name="storyId">The story identifier.</param>
    /// <param name="commentId">The comment identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    [HttpDelete("{commentId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(
        Guid storyId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteCommentCommand(commentId, CurrentUserId), cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// Request body for adding a comment.
/// </summary>
/// <param name="Body">The comment text.</param>
public record AddCommentRequest(string Body);
