using BookingPlatformInsiders.Application.Rooms.Dtos;

namespace BookingPlatformInsiders.Application.Rooms;

public interface IRoomQueries
{
    Task<(IReadOnlyList<RoomDto> Items, int TotalCount)> GetPageAsync(
        int offset,
        int limit,
        CancellationToken cancellationToken = default);
}
