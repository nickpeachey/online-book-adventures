using MediatR;

namespace OnlineBookAdventures.Application.Features.Choices.Commands.DeleteChoice;

/// <summary>
/// Command to delete a choice between two nodes.
/// </summary>
/// <param name="ChoiceId">The identifier of the choice to delete.</param>
/// <param name="RequestingUserId">The identifier of the requesting user.</param>
public record DeleteChoiceCommand(Guid ChoiceId, Guid RequestingUserId) : IRequest;
