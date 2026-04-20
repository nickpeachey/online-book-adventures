using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Application.Features.Nodes.Commands.CreateNode;

/// <summary>
/// Handles the <see cref="CreateNodeCommand"/>.
/// </summary>
public sealed class CreateNodeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateNodeCommand, Guid>
{
    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateNodeCommand request, CancellationToken cancellationToken)
    {
        var story = await context.Stories
            .FirstOrDefaultAsync(s => s.Id == request.StoryId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Story '{request.StoryId}' not found.");

        if (story.AuthorId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the story author can add nodes.");

        var node = new StoryNode
        {
            StoryId = request.StoryId,
            Title = request.Title,
            Content = request.Content,
            IsStart = request.IsStart,
            IsEnd = request.IsEnd,
            PositionX = request.PositionX,
            PositionY = request.PositionY
        };

        context.StoryNodes.Add(node);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return node.Id;
    }
}
