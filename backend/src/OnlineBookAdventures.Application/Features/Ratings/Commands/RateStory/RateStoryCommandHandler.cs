using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Application.Events;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Application.Features.Ratings.Commands.RateStory;

/// <summary>
/// Handles the <see cref="RateStoryCommand"/> by upserting a rating record and publishing an event.
/// </summary>
public sealed class RateStoryCommandHandler(IApplicationDbContext context, IEventPublisher eventPublisher)
    : IRequestHandler<RateStoryCommand>
{
    /// <inheritdoc/>
    public async Task<Unit> Handle(RateStoryCommand request, CancellationToken cancellationToken)
    {
        var storyExists = await context.Stories
            .AnyAsync(s => s.Id == request.StoryId, cancellationToken)
            .ConfigureAwait(false);

        if (!storyExists)
            throw new KeyNotFoundException($"Story '{request.StoryId}' not found.");

        var existing = await context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == request.UserId && r.StoryId == request.StoryId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            context.Ratings.Add(new Rating
            {
                UserId = request.UserId,
                StoryId = request.StoryId,
                Score = request.Score
            });
        }
        else
        {
            existing.Score = request.Score;
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await eventPublisher.PublishAsync(
            new StoryRatedEvent(request.UserId, request.StoryId, request.Score, DateTimeOffset.UtcNow),
            cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
