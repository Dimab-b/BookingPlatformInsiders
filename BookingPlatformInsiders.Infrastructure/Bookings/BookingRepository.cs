using BookingPlatformInsiders.Application.Bookings;
using BookingPlatformInsiders.Domain.Bookings;
using BookingPlatformInsiders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatformInsiders.Infrastructure.Bookings;

internal sealed class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await _context.Bookings.AddAsync(booking, cancellationToken);
    }

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Bookings.FirstOrDefaultAsync(booking => booking.Id == id, cancellationToken);
    }

    public Task<Booking?> GetByClientRequestIdAsync(
        Guid ownerId,
        string clientRequestId,
        CancellationToken cancellationToken = default)
    {
        return _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                booking => booking.OwnerId == ownerId
                    && booking.ClientRequestId == clientRequestId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetOverlapCandidatesAsync(
        Guid roomId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(booking =>
                booking.RoomId == roomId
                && (booking.Status == BookingStatus.Scheduled
                    || booking.Status == BookingStatus.PendingPayment
                    || booking.Status == BookingStatus.Completed)
                && booking.StartUtc < endUtc
                && booking.EndUtc > startUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetExpiredScheduledAsync(
        DateTime nowUtc,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Where(booking => booking.Status == BookingStatus.Scheduled && booking.EndUtc <= nowUtc)
            .OrderBy(booking => booking.EndUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }
}
