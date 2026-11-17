namespace BookingPlatformInsiders.Application.Payments.Dtos;

public sealed record PaymentStatusDto(
    Guid BookingId,
    string BookingStatus,
    DateTime? PaymentExpiresAtUtc,
    PaymentDto? LatestPayment,
    int AttemptCount,
    int MaxAttempts);
