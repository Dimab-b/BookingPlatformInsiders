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

public sealed class InvalidBookingOwnerException : DomainException
{
    public InvalidBookingOwnerException()
        : base(ErrorCodes.BookingOwnerInvalid, "Booking owner is required.")
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
        : base(ErrorCodes.BookingCannotCancel, "Only scheduled or pending bookings can be cancelled.")
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

public sealed class InvalidBookingPriceException : DomainException
{
    public InvalidBookingPriceException()
        : base(ErrorCodes.BookingPriceInvalid, "Booking amount must be positive with at most two decimals.")
    {
    }
}

public sealed class InvalidBookingCurrencyException : DomainException
{
    public InvalidBookingCurrencyException()
        : base(ErrorCodes.BookingCurrencyInvalid, "Currency must be a three-letter ISO code.")
    {
    }
}

public sealed class InvalidBookingExchangeRateException : DomainException
{
    public InvalidBookingExchangeRateException()
        : base(ErrorCodes.BookingExchangeRateInvalid, "Exchange rate must be positive with at most eight decimals.")
    {
    }
}

public sealed class InvalidBookingRateProviderException : DomainException
{
    public InvalidBookingRateProviderException()
        : base(ErrorCodes.BookingRateProviderInvalid, "Rate provider is required and must be at most 50 characters.")
    {
    }
}

public sealed class InvalidBookingRateCapturedAtException : DomainException
{
    public InvalidBookingRateCapturedAtException()
        : base(ErrorCodes.BookingRateCapturedAtInvalid, "Rate capture time must be in UTC.")
    {
    }
}

public sealed class InvalidBookingPaymentExpiryException : DomainException
{
    public InvalidBookingPaymentExpiryException()
        : base(ErrorCodes.BookingPaymentExpiryInvalid, "Payment expiration must be a future UTC time.")
    {
    }
}

public sealed class InvalidBookingClientRequestIdException : DomainException
{
    public InvalidBookingClientRequestIdException()
        : base(ErrorCodes.BookingClientRequestInvalid, "Client request id must be at most 100 characters.")
    {
    }
}

public sealed class InvalidBookingPaymentStateException : DomainException
{
    public InvalidBookingPaymentStateException(string message)
        : base(ErrorCodes.BookingNotPendingPayment, message)
    {
    }
}
