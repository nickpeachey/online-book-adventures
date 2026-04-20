using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Comments.Queries.ListComments;

/// <summary>
/// Handles the <see cref="ListCommentsQuery"/>.
/// </summary>
public sealed class ListCommentsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<ListCommentsQuery, ListCommentsResult>
{
    /// <inheritdoc/>
    public async Task<ListCommentsResult> Handle(ListCommentsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Comments
            .Include(c => c.User)
            .Where(c => c.StoryId == request.StoryId)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var comments = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CommentDto(c.Id, c.UserId, c.User.Username, c.Body, c.CreatedAt, c.EditedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new ListCommentsResult(comments, totalCount, request.Page, request.PageSize);
    }
}
