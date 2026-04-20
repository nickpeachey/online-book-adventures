using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBookAdventures.Application.Common.Interfaces;
using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IRequestHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await context.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken)
            .ConfigureAwait(false);

        if (emailExists)
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");

        var usernameExists = await context.Users
            .AnyAsync(u => u.Username == request.Username, cancellationToken)
            .ConfigureAwait(false);

        if (usernameExists)
            throw new InvalidOperationException($"Username '{request.Username}' is already taken.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var token = tokenService.GenerateAccessToken(user);
        return new RegisterResult(user.Id, user.Username, token);
    }
}
