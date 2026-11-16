namespace BookingPlatformInsiders.Application.Rooms;

public interface IRoomRepository
{
    Task<bool> ExistsAsync(Guid roomId, CancellationToken cancellationToken = default);
}
