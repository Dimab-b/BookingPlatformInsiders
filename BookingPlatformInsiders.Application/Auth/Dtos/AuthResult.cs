namespace BookingPlatformInsiders.Application.Auth.Dtos;

public sealed record AuthResult(
    string AccessToken,
    int ExpiresInSeconds,
    string RefreshToken,
    UserDto User);
