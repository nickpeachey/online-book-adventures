using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Application.Events;

namespace OnlineBookAdventures.Application.Features.Progress.Commands.StartStory;

/// <summary>
/// Handles the <see cref="StartStoryCommand"/> by creating or resetting a progress record.
/// </summary>
public sealed class StartStoryCommandHandler(IApplicationDbContext context, IEventPublisher eventPublisher)
    : IRequestHandler<StartStoryCommand, Guid>
{
    /// <inheritdoc/>
    public async Task<Guid> Handle(StartStoryCommand request, CancellationToken cancellationToken)
    {
        var startNode = await context.StoryNodes
            .FirstOrDefaultAsync(n => n.StoryId == request.StoryId && n.IsStart, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Story '{request.StoryId}' has no start node.");

        var progress = await context.Progresses
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.StoryId == request.StoryId, cancellationToken)
            .ConfigureAwait(false);

        if (progress is null)
        {
            progress = new Domain.Entities.Progress
            {
                UserId = request.UserId,
                StoryId = request.StoryId,
                CurrentNodeId = startNode.Id
            };
            context.Progresses.Add(progress);
        }
        else
        {
            progress.CurrentNodeId = startNode.Id;
            progress.IsCompleted = false;
            progress.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await eventPublisher.PublishAsync(
            new StoryStartedEvent(request.UserId, request.StoryId, DateTimeOffset.UtcNow),
            cancellationToken).ConfigureAwait(false);

        return progress.CurrentNodeId;
    }
}
