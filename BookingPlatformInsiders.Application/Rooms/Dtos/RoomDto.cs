namespace BookingPlatformInsiders.Application.Rooms.Dtos;

public sealed record RoomDto(
    Guid Id,
    string Name,
    int Capacity,
    string Location,
    string? Description,
    bool IsPaid);
