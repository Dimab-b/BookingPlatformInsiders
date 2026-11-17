using BookingPlatformInsiders.Application.Common.Exceptions;
using BookingPlatformInsiders.Domain.Common.Errors;

namespace BookingPlatformInsiders.Application.Auth;

public sealed class EmailAlreadyTakenException : UseCaseException
{
    public EmailAlreadyTakenException()
        : base(ErrorCodes.AuthEmailTaken, "Email is already registered.")
    {
    }
}

public sealed class InvalidCredentialsException : UseCaseException
{
    public InvalidCredentialsException()
        : base(ErrorCodes.AuthInvalidCredentials, "Invalid email or password.")
    {
    }
}

public sealed class InvalidRefreshTokenRequestException : UseCaseException
{
    public InvalidRefreshTokenRequestException()
        : base(ErrorCodes.AuthInvalidToken, "The refresh token is invalid or expired.")
    {
    }
}
