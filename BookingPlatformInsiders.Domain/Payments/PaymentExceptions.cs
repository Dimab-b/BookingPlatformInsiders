using BookingPlatformInsiders.Domain.Common.Errors;
using BookingPlatformInsiders.Domain.Common.Exceptions;

namespace BookingPlatformInsiders.Domain.Payments;

public sealed class InvalidPaymentException : DomainException
{
    public InvalidPaymentException()
        : base(ErrorCodes.PaymentInvalid, "Payment data is invalid.")
    {
    }
}

public sealed class InvalidPaymentStateException : DomainException
{
    public InvalidPaymentStateException(string message)
        : base(ErrorCodes.PaymentInvalidState, message)
    {
    }
}
