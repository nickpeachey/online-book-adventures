using MediatR;

namespace OnlineBookAdventures.Application.Features.Choices.Commands.CreateChoice;

/// <summary>
/// Command to create a choice connecting two story nodes.
/// </summary>
/// <param name="FromNodeId">The source node identifier.</param>
/// <param name="ToNodeId">The destination node identifier.</param>
/// <param name="Label">The choice label shown to the reader.</param>
/// <param name="Order">The display order among choices on the same node.</param>
/// <param name="RequestingUserId">The identifier of the requesting user.</param>
public record CreateChoiceCommand(Guid FromNodeId, Guid ToNodeId, string Label, int Order, Guid RequestingUserId) : IRequest<Guid>;
