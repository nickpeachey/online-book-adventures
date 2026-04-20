using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Progress.Commands.ResetProgress;

/// <summary>
/// Handles the <see cref="ResetProgressCommand"/> by removing the progress record.
/// </summary>
public sealed class ResetProgressCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ResetProgressCommand>
{
    /// <inheritdoc/>
    public async Task<Unit> Handle(ResetProgressCommand request, CancellationToken cancellationToken)
    {
        var progress = await context.Progresses
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.StoryId == request.StoryId, cancellationToken)
            .ConfigureAwait(false);

        if (progress is not null)
        {
            context.Progresses.Remove(progress);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Unit.Value;
    }
}
