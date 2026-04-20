using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Application.Features.Stories.Queries.Dtos;

namespace OnlineBookAdventures.Application.Features.Stories.Queries.GetStoryGraph;

/// <summary>
/// Handles the <see cref="GetStoryGraphQuery"/>, with Redis caching.
/// </summary>
public sealed class GetStoryGraphQueryHandler(
    IApplicationDbContext context,
    IStoryGraphCache cache) : IRequestHandler<GetStoryGraphQuery, StoryGraphDto>
{
    /// <inheritdoc/>
    public async Task<StoryGraphDto> Handle(GetStoryGraphQuery request, CancellationToken cancellationToken)
    {
        var cached = await cache.GetAsync(request.StoryId, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
            return cached;

        var story = await context.Stories
            .Include(s => s.Author)
            .FirstOrDefaultAsync(s => s.Id == request.StoryId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Story '{request.StoryId}' not found.");

        var nodes = await context.StoryNodes
            .Where(n => n.StoryId == request.StoryId)
            .Select(n => new NodeDto(n.Id, n.StoryId, n.Title, n.Content, n.IsStart, n.IsEnd, n.PositionX, n.PositionY))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var nodeIds = nodes.Select(n => n.Id).ToHashSet();

        var choices = await context.Choices
            .Where(c => nodeIds.Contains(c.FromNodeId))
            .OrderBy(c => c.Order)
            .Select(c => new ChoiceDto(c.Id, c.FromNodeId, c.ToNodeId, c.Label, c.Order))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var storyDto = new StoryDto(
            story.Id, story.AuthorId, story.Author.Username,
            story.Title, story.Description, story.CoverImageUrl,
            story.IsPublished, story.CreatedAt, story.UpdatedAt);

        var graph = new StoryGraphDto(storyDto, nodes, choices);

        await cache.SetAsync(request.StoryId, graph, cancellationToken).ConfigureAwait(false);

        return graph;
    }
}
