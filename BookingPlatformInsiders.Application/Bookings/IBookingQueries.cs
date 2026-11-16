using BookingPlatformInsiders.Application.Bookings.Dtos;

namespace BookingPlatformInsiders.Application.Bookings;

public interface IBookingQueries
{
    Task<(IReadOnlyList<BookingDto> Items, int TotalCount)> GetSchedulePageAsync(
        Guid roomId,
        DateTime dayStartUtc,
        DateTime dayEndUtc,
        int offset,
        int limit,
        CancellationToken cancellationToken = default);
}
