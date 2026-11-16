using BookingPlatformInsiders.Application.Rooms;
using BookingPlatformInsiders.Domain.Rooms;
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

    public async Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Rooms
            .AsNoTracking()
            .OrderBy(room => room.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        return _context.Rooms.AnyAsync(room => room.Id == roomId, cancellationToken);
    }
}
