using MediatR;

namespace OnlineBookAdventures.Application.Features.Comments.Commands.AddComment;

/// <summary>
/// Command to add a new comment to a story.
/// </summary>
/// <param name="UserId">The identifier of the commenting user.</param>
/// <param name="StoryId">The identifier of the story.</param>
/// <param name="Body">The comment text.</param>
public record AddCommentCommand(Guid UserId, Guid StoryId, string Body) : IRequest<Guid>;
