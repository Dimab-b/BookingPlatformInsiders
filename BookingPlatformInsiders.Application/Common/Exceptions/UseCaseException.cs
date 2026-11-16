using BookingPlatformInsiders.Domain.Common.Errors;

namespace BookingPlatformInsiders.Application.Common.Exceptions;

public abstract class UseCaseException : Exception
{
    public string Code { get; }

    protected UseCaseException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}

public sealed class RoomNotFoundException : UseCaseException
{
    public RoomNotFoundException()
        : base(ErrorCodes.RoomNotFound, "Room not found.")
    {
    }
}

public sealed class BookingNotFoundException : UseCaseException
{
    public BookingNotFoundException()
        : base(ErrorCodes.BookingNotFound, "Booking not found.")
    {
    }
}

public sealed class ConcurrencyConflictException : UseCaseException
{
    public ConcurrencyConflictException()
        : base(ErrorCodes.ConcurrencyConflict, "The resource was modified by another request.")
    {
    }
}
