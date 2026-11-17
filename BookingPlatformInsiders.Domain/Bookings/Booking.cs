using BookingPlatformInsiders.Domain.Common;

namespace BookingPlatformInsiders.Domain.Bookings;

public sealed class Booking : Entity, IAggregateRoot
{
    public const int MaxClientRequestIdLength = 100;

    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public Guid RoomId { get; private set; }
    public BookingTitle Title { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public BookingStatus Status { get; private set; }
    public BookingPrice? Price { get; private set; }
    public DateTime? PaymentExpiresAtUtc { get; private set; }
    public string? ClientRequestId { get; private set; }

    public bool RequiresPayment => PaymentExpiresAtUtc is not null;

    private Booking()
    {
        Title = null!;
    }

    private Booking(
        Guid id,
        Guid ownerId,
        Guid roomId,
        BookingTitle title,
        DateTime startUtc,
        DateTime endUtc,
        BookingStatus status,
        BookingPrice? price,
        DateTime? paymentExpiresAtUtc,
        string? clientRequestId)
    {
        Id = id;
        OwnerId = ownerId;
        RoomId = roomId;
        Title = title;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Status = status;
        Price = price;
        PaymentExpiresAtUtc = paymentExpiresAtUtc;
        ClientRequestId = clientRequestId;
    }

    public static Booking Create(
        Guid ownerId,
        Guid roomId,
        BookingTitle title,
        DateTime startUtc,
        DateTime endUtc,
        DateTime nowUtc,
        string? clientRequestId = null)
    {
        ValidateSchedule(ownerId, roomId, title, startUtc, endUtc, nowUtc);

        return new Booking(
            Guid.NewGuid(),
            ownerId,
            roomId,
            title,
            startUtc,
            endUtc,
            BookingStatus.Scheduled,
            null,
            null,
            NormalizeClientRequestId(clientRequestId));
    }

    public static Booking CreatePendingPayment(
        Guid ownerId,
        Guid roomId,
        BookingTitle title,
        DateTime startUtc,
        DateTime endUtc,
        DateTime nowUtc,
        BookingPrice price,
        DateTime paymentExpiresAtUtc,
        string? clientRequestId = null)
    {
        ValidateSchedule(ownerId, roomId, title, startUtc, endUtc, nowUtc);

        if (price is null)
        {
            throw new InvalidBookingPriceException();
        }

        if (paymentExpiresAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidBookingPaymentExpiryException();
        }

        if (paymentExpiresAtUtc <= nowUtc)
        {
            throw new InvalidBookingPaymentExpiryException();
        }

        return new Booking(
            Guid.NewGuid(),
            ownerId,
            roomId,
            title,
            startUtc,
            endUtc,
            BookingStatus.PendingPayment,
            price,
            paymentExpiresAtUtc,
            NormalizeClientRequestId(clientRequestId));
    }

    public bool OverlapsWith(Guid roomId, DateTime startUtc, DateTime endUtc) =>
        RoomId == roomId
        && Status is BookingStatus.Scheduled
            or BookingStatus.PendingPayment
            or BookingStatus.Completed
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

    public void ConfirmPayment()
    {
        if (Status == BookingStatus.Scheduled)
        {
            return;
        }

        if (Status != BookingStatus.PendingPayment)
        {
            throw new InvalidBookingPaymentStateException(
                "Only a booking pending payment can be confirmed.");
        }

        Status = BookingStatus.Scheduled;
    }

    public void MarkPaymentFailed()
    {
        if (Status == BookingStatus.PaymentFailed)
        {
            return;
        }

        if (Status != BookingStatus.PendingPayment)
        {
            throw new InvalidBookingPaymentStateException(
                "Only a booking pending payment can be marked as failed.");
        }

        Status = BookingStatus.PaymentFailed;
    }

    public void Expire(DateTime nowUtc)
    {
        if (Status == BookingStatus.Expired)
        {
            return;
        }

        if (Status != BookingStatus.PendingPayment)
        {
            throw new InvalidBookingPaymentStateException(
                "Only a booking pending payment can expire.");
        }

        if (nowUtc.Kind != DateTimeKind.Utc)
        {
            throw new BookingTimeNotUtcException();
        }

        if (PaymentExpiresAtUtc is null || nowUtc < PaymentExpiresAtUtc.Value)
        {
            throw new InvalidBookingPaymentStateException(
                "The payment window has not expired yet.");
        }

        Status = BookingStatus.Expired;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
        {
            return;
        }

        if (Status is not (BookingStatus.Scheduled or BookingStatus.PendingPayment))
        {
            throw new BookingCannotCancelException();
        }

        Status = BookingStatus.Cancelled;
    }

    public void Complete(DateTime nowUtc)
    {
        if (Status == BookingStatus.Completed)
        {
            return;
        }

        if (Status != BookingStatus.Scheduled)
        {
            throw new BookingCannotCompleteException("Only an active booking can be completed.");
        }

        if (EndUtc > nowUtc)
        {
            throw new BookingCannotCompleteException("A booking cannot be completed before its end time.");
        }

        Status = BookingStatus.Completed;
    }

    private static void ValidateSchedule(
        Guid ownerId,
        Guid roomId,
        BookingTitle title,
        DateTime startUtc,
        DateTime endUtc,
        DateTime nowUtc)
    {
        if (ownerId == Guid.Empty)
        {
            throw new InvalidBookingOwnerException();
        }

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
    }

    private static string? NormalizeClientRequestId(string? clientRequestId)
    {
        if (string.IsNullOrWhiteSpace(clientRequestId))
        {
            return null;
        }

        var trimmed = clientRequestId.Trim();

        if (trimmed.Length > MaxClientRequestIdLength)
        {
            throw new InvalidBookingClientRequestIdException();
        }

        return trimmed;
    }
}
