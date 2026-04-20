using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Application.Events;

namespace OnlineBookAdventures.Application.Features.Progress.Commands.MakeChoice;

/// <summary>
/// Handles the <see cref="MakeChoiceCommand"/> by advancing the reader to the next node.
/// </summary>
public sealed class MakeChoiceCommandHandler(IApplicationDbContext context, IEventPublisher eventPublisher)
    : IRequestHandler<MakeChoiceCommand, MakeChoiceResult>
{
    /// <inheritdoc/>
    public async Task<MakeChoiceResult> Handle(MakeChoiceCommand request, CancellationToken cancellationToken)
    {
        var progress = await context.Progresses
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.StoryId == request.StoryId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("No active progress found. Start the story first.");

        var choice = await context.Choices
            .Include(c => c.ToNode)
            .FirstOrDefaultAsync(c => c.Id == request.ChoiceId && c.FromNodeId == progress.CurrentNodeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Choice is not valid from the current node.");

        var fromNodeId = progress.CurrentNodeId;
        progress.CurrentNodeId = choice.ToNodeId;
        progress.IsCompleted = choice.ToNode.IsEnd;
        progress.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await eventPublisher.PublishAsync(
            new ChoiceMadeEvent(request.UserId, request.StoryId, request.ChoiceId, fromNodeId, choice.ToNodeId, DateTimeOffset.UtcNow),
            cancellationToken).ConfigureAwait(false);

        if (choice.ToNode.IsEnd)
        {
            await eventPublisher.PublishAsync(
                new StoryCompletedEvent(request.UserId, request.StoryId, choice.ToNodeId, DateTimeOffset.UtcNow),
                cancellationToken).ConfigureAwait(false);
        }

        return new MakeChoiceResult(choice.ToNodeId, choice.ToNode.IsEnd);
    }
}
