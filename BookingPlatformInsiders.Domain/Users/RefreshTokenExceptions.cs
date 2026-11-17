using BookingPlatformInsiders.Domain.Common.Errors;
using BookingPlatformInsiders.Domain.Common.Exceptions;

namespace BookingPlatformInsiders.Domain.Users;

public sealed class InvalidRefreshTokenException : DomainException
{
    public InvalidRefreshTokenException()
        : base(ErrorCodes.AuthInvalidToken, "The refresh token is invalid.")
    {
    }
}

public sealed class InvalidRefreshTokenStateException : DomainException
{
    public InvalidRefreshTokenStateException(string message)
        : base(ErrorCodes.AuthInvalidToken, message)
    {
    }
}

public sealed class RefreshTokenExpiredException : DomainException
{
    public RefreshTokenExpiredException()
        : base(ErrorCodes.AuthSessionExpired, "The refresh token has expired.")
    {
    }
}
