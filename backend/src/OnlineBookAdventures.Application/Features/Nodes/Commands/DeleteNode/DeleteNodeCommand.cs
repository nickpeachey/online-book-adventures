using MediatR;

namespace OnlineBookAdventures.Application.Features.Nodes.Commands.DeleteNode;

/// <summary>
/// Command to delete a story node and its associated choices.
/// </summary>
/// <param name="NodeId">The identifier of the node to delete.</param>
/// <param name="RequestingUserId">The identifier of the requesting user.</param>
public record DeleteNodeCommand(Guid NodeId, Guid RequestingUserId) : IRequest;
