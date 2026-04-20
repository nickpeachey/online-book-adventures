using MediatR;

namespace OnlineBookAdventures.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;
public record LoginResult(Guid UserId, string Username, string AccessToken);
