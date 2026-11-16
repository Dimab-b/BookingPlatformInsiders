using BookingPlatformInsiders.Application.Rooms;
using BookingPlatformInsiders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatformInsiders.Infrastructure.Rooms;

internal sealed class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        return _context.Rooms.AnyAsync(room => room.Id == roomId, cancellationToken);
    }
}
