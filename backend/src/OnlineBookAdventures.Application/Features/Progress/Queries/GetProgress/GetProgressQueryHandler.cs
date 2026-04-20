using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Progress.Queries.GetProgress;

/// <summary>
/// Handles the <see cref="GetProgressQuery"/>.
/// </summary>
public sealed class GetProgressQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProgressQuery, ProgressDto?>
{
    /// <inheritdoc/>
    public async Task<ProgressDto?> Handle(GetProgressQuery request, CancellationToken cancellationToken)
    {
        var progress = await context.Progresses
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.StoryId == request.StoryId, cancellationToken)
            .ConfigureAwait(false);

        if (progress is null)
            return null;

        return new ProgressDto(
            progress.Id,
            progress.UserId,
            progress.StoryId,
            progress.CurrentNodeId,
            progress.IsCompleted,
            progress.UpdatedAt);
    }
}
