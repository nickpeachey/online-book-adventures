using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Nodes.Commands.DeleteNode;

/// <summary>
/// Handles the <see cref="DeleteNodeCommand"/>.
/// </summary>
public sealed class DeleteNodeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteNodeCommand>
{
    /// <inheritdoc/>
    public async Task<Unit> Handle(DeleteNodeCommand request, CancellationToken cancellationToken)
    {
        var node = await context.StoryNodes
            .Include(n => n.Story)
            .FirstOrDefaultAsync(n => n.Id == request.NodeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Node '{request.NodeId}' not found.");

        if (node.Story.AuthorId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the story author can delete nodes.");

        context.StoryNodes.Remove(node);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
