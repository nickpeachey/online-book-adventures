using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Application.Features.Comments.Commands.AddComment;

/// <summary>
/// Handles the <see cref="AddCommentCommand"/> by persisting a new comment.
/// </summary>
public sealed class AddCommentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AddCommentCommand, Guid>
{
    /// <inheritdoc/>
    public async Task<Guid> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var storyExists = await context.Stories
            .AnyAsync(s => s.Id == request.StoryId, cancellationToken)
            .ConfigureAwait(false);

        if (!storyExists)
            throw new KeyNotFoundException($"Story '{request.StoryId}' not found.");

        var comment = new Comment
        {
            UserId = request.UserId,
            StoryId = request.StoryId,
            Body = request.Body
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return comment.Id;
    }
}
