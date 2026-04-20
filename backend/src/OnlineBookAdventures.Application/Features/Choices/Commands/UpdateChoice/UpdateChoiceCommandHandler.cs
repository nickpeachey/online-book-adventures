using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Choices.Commands.UpdateChoice;

/// <summary>
/// Handles the <see cref="UpdateChoiceCommand"/>.
/// </summary>
public sealed class UpdateChoiceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateChoiceCommand>
{
    /// <inheritdoc/>
    public async Task<Unit> Handle(UpdateChoiceCommand request, CancellationToken cancellationToken)
    {
        var choice = await context.Choices
            .Include(c => c.FromNode).ThenInclude(n => n.Story)
            .FirstOrDefaultAsync(c => c.Id == request.ChoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Choice '{request.ChoiceId}' not found.");

        if (choice.FromNode.Story.AuthorId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the story author can update choices.");

        choice.Label = request.Label;
        choice.Order = request.Order;

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
