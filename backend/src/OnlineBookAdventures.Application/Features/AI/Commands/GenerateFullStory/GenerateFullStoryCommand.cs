using MediatR;

namespace OnlineBookAdventures.Application.Features.AI.Commands.GenerateFullStory;

public record GenerateFullStoryCommand(Guid AuthorId, string Prompt) : IRequest<Guid>;
