namespace BookingPlatformInsiders.Application.Payments.Dtos;

public sealed record InitiatePaymentResult(
    Guid PaymentId,
    Guid BookingId,
    int AttemptNumber,
    string Provider,
    string Status,
    string? PaymentUrl,
    DateTime PaymentExpiresAtUtc);
