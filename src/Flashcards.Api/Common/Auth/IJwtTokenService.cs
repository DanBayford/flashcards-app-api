using Flashcards.Api.Features.Users;

namespace Flashcards.Api.Common.Auth;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    TimeSpan RefreshTokenLifetime { get; }
}