using MediatR;

namespace OnlineBookAdventures.Application.Features.Comments.Commands.DeleteComment;

/// <summary>
/// Command to delete a comment.
/// </summary>
/// <param name="CommentId">The identifier of the comment to delete.</param>
/// <param name="RequestingUserId">The identifier of the requesting user (must be the comment author).</param>
public record DeleteCommentCommand(Guid CommentId, Guid RequestingUserId) : IRequest;
