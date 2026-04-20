using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Application.Features.Choices.Commands.CreateChoice;

/// <summary>
/// Handles the <see cref="CreateChoiceCommand"/>.
/// </summary>
public sealed class CreateChoiceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateChoiceCommand, Guid>
{
    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateChoiceCommand request, CancellationToken cancellationToken)
    {
        var fromNode = await context.StoryNodes
            .Include(n => n.Story)
            .FirstOrDefaultAsync(n => n.Id == request.FromNodeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Source node '{request.FromNodeId}' not found.");

        if (fromNode.Story.AuthorId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the story author can add choices.");

        var toNodeExists = await context.StoryNodes
            .AnyAsync(n => n.Id == request.ToNodeId && n.StoryId == fromNode.StoryId, cancellationToken)
            .ConfigureAwait(false);

        if (!toNodeExists)
            throw new KeyNotFoundException($"Destination node '{request.ToNodeId}' not found in the same story.");

        var choice = new Choice
        {
            FromNodeId = request.FromNodeId,
            ToNodeId = request.ToNodeId,
            Label = request.Label,
            Order = request.Order
        };

        context.Choices.Add(choice);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return choice.Id;
    }
}
