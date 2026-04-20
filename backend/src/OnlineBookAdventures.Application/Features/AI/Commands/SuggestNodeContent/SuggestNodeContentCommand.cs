using MediatR;

namespace OnlineBookAdventures.Application.Features.AI.Commands.SuggestNodeContent;

public record SuggestNodeContentCommand(Guid StoryId, Guid UserId, string NodeTitle, string CurrentContent) : IRequest<string>;
