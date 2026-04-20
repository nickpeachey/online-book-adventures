using MediatR;

namespace OnlineBookAdventures.Application.Features.Choices.Commands.UpdateChoice;

/// <summary>
/// Command to update a choice's label and order.
/// </summary>
/// <param name="ChoiceId">The identifier of the choice to update.</param>
/// <param name="RequestingUserId">The identifier of the requesting user.</param>
/// <param name="Label">The new choice label.</param>
/// <param name="Order">The new display order.</param>
public record UpdateChoiceCommand(Guid ChoiceId, Guid RequestingUserId, string Label, int Order) : IRequest;
