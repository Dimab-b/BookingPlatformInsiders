using BookingPlatformInsiders.Domain.Common.Errors;
using BookingPlatformInsiders.Domain.Common.Exceptions;

namespace BookingPlatformInsiders.Domain.Bookings;

public sealed class InvalidBookingTitleException : DomainException
{
    public InvalidBookingTitleException()
        : base(ErrorCodes.BookingTitleInvalid, "Booking title is required and must be at most 200 characters.")
    {
    }
}

public sealed class InvalidBookingRoomException : DomainException
{
    public InvalidBookingRoomException()
        : base(ErrorCodes.BookingRoomInvalid, "Booking room is required.")
    {
    }
}

public sealed class InvalidBookingTimeRangeException : DomainException
{
    public InvalidBookingTimeRangeException()
        : base(ErrorCodes.BookingTimeRangeInvalid, "End time must be after start time.")
    {
    }
}

public sealed class BookingTimeNotUtcException : DomainException
{
    public BookingTimeNotUtcException()
        : base(ErrorCodes.BookingTimeNotUtc, "Start and end times must be in UTC.")
    {
    }
}

public sealed class BookingStartInPastException : DomainException
{
    public BookingStartInPastException()
        : base(ErrorCodes.BookingStartInPast, "A booking cannot start in the past.")
    {
    }
}

public sealed class BookingOverlapException : DomainException
{
    public BookingOverlapException()
        : base(ErrorCodes.BookingSlotTaken, "The room already has a booking that overlaps the requested time.")
    {
    }
}

public sealed class BookingCannotCancelException : DomainException
{
    public BookingCannotCancelException()
        : base(ErrorCodes.BookingCannotCancel, "A completed booking cannot be cancelled.")
    {
    }
}

public sealed class BookingCannotCompleteException : DomainException
{
    public BookingCannotCompleteException(string message)
        : base(ErrorCodes.BookingCannotComplete, message)
    {
    }
}
