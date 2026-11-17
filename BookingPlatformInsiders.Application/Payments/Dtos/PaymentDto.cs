namespace BookingPlatformInsiders.Application.Payments.Dtos;

public sealed record PaymentDto(
    Guid Id,
    Guid BookingId,
    string Provider,
    string? ProviderPaymentId,
    int AttemptNumber,
    string Status,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
