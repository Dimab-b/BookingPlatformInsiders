using BookingPlatformInsiders.Domain.Common.Errors;
using BookingPlatformInsiders.Domain.Common.Exceptions;

namespace BookingPlatformInsiders.Domain.Users;

public sealed class InvalidEmailException : DomainException
{
    public InvalidEmailException()
        : base(ErrorCodes.UserEmailInvalid, "Email is required and must be a valid address.")
    {
    }
}

public sealed class InvalidPasswordHashException : DomainException
{
    public InvalidPasswordHashException()
        : base(ErrorCodes.UserPasswordHashInvalid, "Password hash is required.")
    {
    }
}

public sealed class InvalidUserRoleException : DomainException
{
    public InvalidUserRoleException()
        : base(ErrorCodes.UserRoleInvalid, "User role is invalid.")
    {
    }
}

public sealed class InvalidUserCreatedAtException : DomainException
{
    public InvalidUserCreatedAtException()
        : base(ErrorCodes.UserCreatedAtInvalid, "User creation time must be in UTC.")
    {
    }
}
