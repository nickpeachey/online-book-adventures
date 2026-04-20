using MediatR;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Application.Features.Stories.Commands.CreateStory;

/// <summary>
/// Handles the <see cref="CreateStoryCommand"/> by persisting a new story.
/// </summary>
public sealed class CreateStoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateStoryCommand, Guid>
{
    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateStoryCommand request, CancellationToken cancellationToken)
    {
        var story = new Story
        {
            AuthorId = request.AuthorId,
            Title = request.Title,
            Description = request.Description
        };

        context.Stories.Add(story);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return story.Id;
    }
}
