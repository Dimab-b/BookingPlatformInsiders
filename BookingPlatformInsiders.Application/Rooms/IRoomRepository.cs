using BookingPlatformInsiders.Domain.Rooms;

namespace BookingPlatformInsiders.Application.Rooms;

public interface IRoomRepository
{
    Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid roomId, CancellationToken cancellationToken = default);
}
