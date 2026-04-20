using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Application.Features.Stories.Queries.Dtos;

namespace OnlineBookAdventures.Application.Features.Stories.Queries.GetStory;

/// <summary>
/// Handles the <see cref="GetStoryQuery"/>.
/// </summary>
public sealed class GetStoryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetStoryQuery, StoryDto>
{
    /// <inheritdoc/>
    public async Task<StoryDto> Handle(GetStoryQuery request, CancellationToken cancellationToken)
    {
        var story = await context.Stories
            .Include(s => s.Author)
            .FirstOrDefaultAsync(s => s.Id == request.StoryId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Story '{request.StoryId}' not found.");

        return new StoryDto(
            story.Id,
            story.AuthorId,
            story.Author.Username,
            story.Title,
            story.Description,
            story.CoverImageUrl,
            story.IsPublished,
            story.CreatedAt,
            story.UpdatedAt);
    }
}
