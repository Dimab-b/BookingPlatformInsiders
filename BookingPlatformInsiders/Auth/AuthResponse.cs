using BookingPlatformInsiders.Application.Auth.Dtos;

namespace BookingPlatformInsiders.Auth;

public sealed record AuthResponse(
    string AccessToken,
    int ExpiresInSeconds,
    UserDto User);
