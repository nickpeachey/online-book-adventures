using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Application.Features.Stories.Queries.Dtos;

namespace OnlineBookAdventures.Application.Features.Stories.Queries.ListStories;

/// <summary>
/// Handles the <see cref="ListStoriesQuery"/>.
/// </summary>
public sealed class ListStoriesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<ListStoriesQuery, ListStoriesResult>
{
    /// <inheritdoc/>
    public async Task<ListStoriesResult> Handle(ListStoriesQuery request, CancellationToken cancellationToken)
    {
        var query = context.Stories
            .Include(s => s.Author)
            .Where(s => s.IsPublished);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(s =>
                s.Title.ToLower().Contains(term) ||
                s.Description.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var stories = await query
            .OrderByDescending(s => s.PublishedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StoryDto(
                s.Id,
                s.AuthorId,
                s.Author.Username,
                s.Title,
                s.Description,
                s.CoverImageUrl,
                s.IsPublished,
                s.CreatedAt,
                s.UpdatedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new ListStoriesResult(stories, totalCount, request.Page, request.PageSize);
    }
}
