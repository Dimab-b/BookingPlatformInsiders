namespace BookingPlatformInsiders.Application.Bookings.Dtos;

public sealed record BookingDto(
    Guid Id,
    Guid RoomId,
    string Title,
    DateTime StartUtc,
    DateTime EndUtc,
    string Status,
    bool IsOwnedByCurrentUser,
    decimal? AmountOriginal,
    string? Currency,
    decimal? AmountEur,
    DateTime? PaymentExpiresAtUtc);
