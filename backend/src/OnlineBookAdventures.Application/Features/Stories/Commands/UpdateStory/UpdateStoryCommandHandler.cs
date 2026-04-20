using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.UpdateStory;

/// <summary>
/// Handles the <see cref="UpdateStoryCommand"/>.
/// </summary>
public sealed class UpdateStoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateStoryCommand>
{
    /// <inheritdoc/>
    public async Task<Unit> Handle(UpdateStoryCommand request, CancellationToken cancellationToken)
    {
        var story = await context.Stories
            .FirstOrDefaultAsync(s => s.Id == request.StoryId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Story '{request.StoryId}' not found.");

        if (story.AuthorId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the story author can update this story.");

        story.Title = request.Title;
        story.Description = request.Description;
        story.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
