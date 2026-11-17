using BookingPlatformInsiders.Application.Common.Exceptions;
using BookingPlatformInsiders.Domain.Common.Errors;

namespace BookingPlatformInsiders.Application.Payments;

public sealed class PaymentExpiredException : UseCaseException
{
    public PaymentExpiredException()
        : base(ErrorCodes.PaymentExpired, "The payment window for this booking has expired.")
    {
    }
}
