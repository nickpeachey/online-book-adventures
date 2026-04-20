using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.DeleteStory;

/// <summary>
/// Handles the <see cref="DeleteStoryCommand"/>.
/// </summary>
public sealed class DeleteStoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteStoryCommand>
{
    /// <inheritdoc/>
    public async Task<Unit> Handle(DeleteStoryCommand request, CancellationToken cancellationToken)
    {
        var story = await context.Stories
            .FirstOrDefaultAsync(s => s.Id == request.StoryId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Story '{request.StoryId}' not found.");

        if (story.AuthorId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the story author can delete this story.");

        context.Stories.Remove(story);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
