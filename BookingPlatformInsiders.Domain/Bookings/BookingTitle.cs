namespace BookingPlatformInsiders.Domain.Bookings;

public sealed record BookingTitle
{
    public const int MaxLength = 200;

    public string Value { get; }

    public BookingTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidBookingTitleException();
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            throw new InvalidBookingTitleException();
        }

        Value = trimmed;
    }

    public override string ToString() => Value;
}
