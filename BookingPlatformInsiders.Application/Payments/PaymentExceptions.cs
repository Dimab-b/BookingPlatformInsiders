using BookingPlatformInsiders.Application.Common.Exceptions;
using BookingPlatformInsiders.Domain.Common.Errors;

namespace BookingPlatformInsiders.Application.Payments;

public sealed class PaymentNotFoundException : UseCaseException
{
    public PaymentNotFoundException()
        : base(ErrorCodes.PaymentNotFound, "Payment not found.")
    {
    }
}

public sealed class PaymentAttemptsExceededException : UseCaseException
{
    public PaymentAttemptsExceededException()
        : base(ErrorCodes.PaymentAttemptsExceeded, "The maximum number of payment attempts has been reached.")
    {
    }
}

public sealed class PaymentNotPendingException : UseCaseException
{
    public PaymentNotPendingException()
        : base(ErrorCodes.PaymentNotPending, "The booking is not waiting for payment.")
    {
    }
}

public sealed class RatesUnavailableException : UseCaseException
{
    public RatesUnavailableException()
        : base(ErrorCodes.RatesUnavailable, "Exchange rates are temporarily unavailable.")
    {
    }
}

public sealed class IdempotencyKeyReusedException : UseCaseException
{
    public IdempotencyKeyReusedException()
        : base(ErrorCodes.IdempotencyKeyReused, "The idempotency key was reused with a different request.")
    {
    }
}

public sealed class InvalidPaymentWebhookException : UseCaseException
{
    public InvalidPaymentWebhookException()
        : base(ErrorCodes.PaymentWebhookInvalid, "The payment webhook signature or payload is invalid.")
    {
    }
}
