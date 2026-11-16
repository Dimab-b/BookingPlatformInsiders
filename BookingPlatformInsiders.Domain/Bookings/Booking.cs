using BookingPlatformInsiders.Domain.Common;

namespace BookingPlatformInsiders.Domain.Bookings;

public sealed class Booking : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public BookingTitle Title { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public BookingStatus Status { get; private set; }

    private Booking()
    {
        Title = null!;
    }

    private Booking(Guid id, Guid roomId, BookingTitle title, DateTime startUtc, DateTime endUtc)
    {
        Id = id;
        RoomId = roomId;
        Title = title;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Status = BookingStatus.Scheduled;
    }

    public static Booking Create(
        Guid roomId,
        BookingTitle title,
        DateTime startUtc,
        DateTime endUtc,
        DateTime nowUtc)
    {
        if (roomId == Guid.Empty)
        {
            throw new InvalidBookingRoomException();
        }

        if (title is null)
        {
            throw new InvalidBookingTitleException();
        }

        if (startUtc.Kind != DateTimeKind.Utc || endUtc.Kind != DateTimeKind.Utc)
        {
            throw new BookingTimeNotUtcException();
        }

        if (endUtc <= startUtc)
        {
            throw new InvalidBookingTimeRangeException();
        }

        if (startUtc <= nowUtc)
        {
            throw new BookingStartInPastException();
        }

        return new Booking(Guid.NewGuid(), roomId, title, startUtc, endUtc);
    }

    public bool OverlapsWith(Guid roomId, DateTime startUtc, DateTime endUtc) =>
        RoomId == roomId
        && Status != BookingStatus.Cancelled
        && StartUtc < endUtc
        && EndUtc > startUtc;

    public static void EnsureNoOverlap(
        IEnumerable<Booking> existingBookings,
        Guid roomId,
        DateTime startUtc,
        DateTime endUtc)
    {
        if (existingBookings.Any(booking => booking.OverlapsWith(roomId, startUtc, endUtc)))
        {
            throw new BookingOverlapException();
        }
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
        {
            return;
        }

        if (Status == BookingStatus.Completed)
        {
            throw new BookingCannotCancelException();
        }

        Status = BookingStatus.Cancelled;
    }

    public void Complete(DateTime nowUtc)
    {
        if (Status == BookingStatus.Cancelled)
        {
            throw new BookingCannotCompleteException("A cancelled booking cannot be completed.");
        }

        if (Status == BookingStatus.Completed)
        {
            return;
        }

        if (EndUtc > nowUtc)
        {
            throw new BookingCannotCompleteException("A booking cannot be completed before its end time.");
        }

        Status = BookingStatus.Completed;
    }
}
