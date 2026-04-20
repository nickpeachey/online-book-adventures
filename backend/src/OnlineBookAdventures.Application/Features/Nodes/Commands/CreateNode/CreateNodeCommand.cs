using MediatR;

namespace OnlineBookAdventures.Application.Features.Nodes.Commands.CreateNode;

/// <summary>
/// Command to add a new node to a story.
/// </summary>
/// <param name="StoryId">The identifier of the story.</param>
/// <param name="RequestingUserId">The identifier of the requesting user (must be story author).</param>
/// <param name="Title">The node title.</param>
/// <param name="Content">The node narrative content.</param>
/// <param name="IsStart">Whether this is the start node.</param>
/// <param name="IsEnd">Whether this is a terminal node.</param>
/// <param name="PositionX">X coordinate in the visual builder.</param>
/// <param name="PositionY">Y coordinate in the visual builder.</param>
public record CreateNodeCommand(
    Guid StoryId,
    Guid RequestingUserId,
    string Title,
    string Content,
    bool IsStart = false,
    bool IsEnd = false,
    double PositionX = 0,
    double PositionY = 0) : IRequest<Guid>;
