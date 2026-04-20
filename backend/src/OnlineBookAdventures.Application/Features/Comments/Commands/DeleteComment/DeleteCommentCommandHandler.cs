using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Comments.Commands.DeleteComment;

/// <summary>
/// Handles the <see cref="DeleteCommentCommand"/> by removing the comment if the requester is the author.
/// </summary>
public sealed class DeleteCommentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteCommentCommand>
{
    /// <inheritdoc/>
    public async Task<Unit> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Comment '{request.CommentId}' not found.");

        if (comment.UserId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the comment author can delete this comment.");

        context.Comments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
