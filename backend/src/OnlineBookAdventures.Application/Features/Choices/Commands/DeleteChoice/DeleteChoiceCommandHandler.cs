using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Application.Features.Choices.Commands.DeleteChoice;

/// <summary>
/// Handles the <see cref="DeleteChoiceCommand"/>.
/// </summary>
public sealed class DeleteChoiceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteChoiceCommand>
{
    /// <inheritdoc/>
    public async Task<Unit> Handle(DeleteChoiceCommand request, CancellationToken cancellationToken)
    {
        var choice = await context.Choices
            .Include(c => c.FromNode).ThenInclude(n => n.Story)
            .FirstOrDefaultAsync(c => c.Id == request.ChoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Choice '{request.ChoiceId}' not found.");

        if (choice.FromNode.Story.AuthorId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the story author can delete choices.");

        context.Choices.Remove(choice);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
