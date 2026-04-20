using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.PublishStory;

/// <summary>
/// Handles the <see cref="PublishStoryCommand"/>.
/// </summary>
public sealed class PublishStoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<PublishStoryCommand>
{
    /// <inheritdoc/>
    public async Task<Unit> Handle(PublishStoryCommand request, CancellationToken cancellationToken)
    {
        var story = await context.Stories
            .FirstOrDefaultAsync(s => s.Id == request.StoryId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Story '{request.StoryId}' not found.");

        if (story.AuthorId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the story author can publish this story.");

        story.IsPublished = request.Publish;
        story.PublishedAt = request.Publish ? DateTimeOffset.UtcNow : null;
        story.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
