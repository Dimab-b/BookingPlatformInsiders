using BookingPlatformInsiders.Domain.Rooms;

namespace BookingPlatformInsiders.Application.Rooms;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid roomId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid roomId, CancellationToken cancellationToken = default);
}
