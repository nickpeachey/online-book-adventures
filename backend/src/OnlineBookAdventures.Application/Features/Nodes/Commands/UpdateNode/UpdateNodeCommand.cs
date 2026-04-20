using MediatR;

namespace OnlineBookAdventures.Application.Features.Nodes.Commands.UpdateNode;

/// <summary>
/// Command to update an existing story node.
/// </summary>
/// <param name="NodeId">The identifier of the node to update.</param>
/// <param name="RequestingUserId">The identifier of the requesting user.</param>
/// <param name="Title">The new title.</param>
/// <param name="Content">The new narrative content.</param>
/// <param name="IsStart">Whether this is the start node.</param>
/// <param name="IsEnd">Whether this is a terminal node.</param>
/// <param name="PositionX">Updated X coordinate.</param>
/// <param name="PositionY">Updated Y coordinate.</param>
public record UpdateNodeCommand(
    Guid NodeId,
    Guid RequestingUserId,
    string Title,
    string Content,
    bool IsStart,
    bool IsEnd,
    double PositionX,
    double PositionY) : IRequest;
