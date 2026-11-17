namespace BookingPlatformInsiders.Domain.Bookings;

public enum BookingStatus
{
    Scheduled = 1,
    Completed = 2,
    Cancelled = 3,
    PendingPayment = 4,
    PaymentFailed = 5,
    Expired = 6
}
