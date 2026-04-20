using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Nodes.Commands.UpdateNode;

/// <summary>
/// Handles the <see cref="UpdateNodeCommand"/>.
/// </summary>
public sealed class UpdateNodeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateNodeCommand>
{
    /// <inheritdoc/>
    public async Task<Unit> Handle(UpdateNodeCommand request, CancellationToken cancellationToken)
    {
        var node = await context.StoryNodes
            .Include(n => n.Story)
            .FirstOrDefaultAsync(n => n.Id == request.NodeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Node '{request.NodeId}' not found.");

        if (node.Story.AuthorId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the story author can update nodes.");

        node.Title = request.Title;
        node.Content = request.Content;
        node.IsStart = request.IsStart;
        node.IsEnd = request.IsEnd;
        node.PositionX = request.PositionX;
        node.PositionY = request.PositionY;

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
