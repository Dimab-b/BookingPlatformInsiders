namespace BookingPlatformInsiders.Application.Auth.Abstractions;

public interface IRefreshTokenGenerator
{
    string Generate();

    string Hash(string refreshToken);
}
