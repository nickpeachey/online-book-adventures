using MediatR;

namespace OnlineBookAdventures.Application.Features.Progress.Commands.MakeChoice;

/// <summary>
/// Command to advance a reader's progress by selecting a choice.
/// </summary>
/// <param name="UserId">The identifier of the reading user.</param>
/// <param name="StoryId">The identifier of the story.</param>
/// <param name="ChoiceId">The identifier of the choice being made.</param>
public record MakeChoiceCommand(Guid UserId, Guid StoryId, Guid ChoiceId) : IRequest<MakeChoiceResult>;

/// <summary>
/// Result after making a choice, including the new current node.
/// </summary>
/// <param name="NewNodeId">The identifier of the newly reached node.</param>
/// <param name="IsEnd">Whether the new node is a terminal (ending) node.</param>
public record MakeChoiceResult(Guid NewNodeId, bool IsEnd);
