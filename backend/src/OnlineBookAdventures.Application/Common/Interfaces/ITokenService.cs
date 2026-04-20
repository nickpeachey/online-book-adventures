using OnlineBookAdventures.Domain.Entities;

namespace OnlineBookAdventures.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
}
