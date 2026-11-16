using BookingPlatformInsiders.Domain.Bookings;

namespace BookingPlatformInsiders.Application.Bookings;

public interface IBookingRepository
{
    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);

    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetOverlapCandidatesAsync(
        Guid roomId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetExpiredScheduledAsync(
        DateTime nowUtc,
        int batchSize,
        CancellationToken cancellationToken = default);
}
