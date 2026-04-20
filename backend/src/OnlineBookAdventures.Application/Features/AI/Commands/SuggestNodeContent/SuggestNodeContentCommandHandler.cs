using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.AI.Commands.SuggestNodeContent;

/// <summary>
/// Handles <see cref="SuggestNodeContentCommand"/> by fetching the story title and
/// delegating to the AI service for a content suggestion.
/// </summary>
public sealed class SuggestNodeContentCommandHandler(
    IStoryGenerationService storyGenerationService,
    IApplicationDbContext context)
    : IRequestHandler<SuggestNodeContentCommand, string>
{
    /// <inheritdoc/>
    public async Task<string> Handle(SuggestNodeContentCommand request, CancellationToken cancellationToken)
    {
        var story = await context.Stories
            .FirstOrDefaultAsync(s => s.Id == request.StoryId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Story '{request.StoryId}' not found.");

        return await storyGenerationService
            .SuggestNodeContentAsync(story.Title, request.NodeTitle, request.CurrentContent, cancellationToken)
            .ConfigureAwait(false);
    }
}
