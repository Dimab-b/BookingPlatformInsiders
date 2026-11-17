using BookingPlatformInsiders.Domain.Users;

namespace BookingPlatformInsiders.Application.Auth.Abstractions;

public sealed record AccessToken(string Token, int ExpiresInSeconds);

public interface IAccessTokenService
{
    AccessToken CreateAccessToken(User user);
}
