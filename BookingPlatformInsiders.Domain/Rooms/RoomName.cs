using BookingPlatformInsiders.Domain.Common.Exceptions;

namespace BookingPlatformInsiders.Domain.Rooms;

public sealed record RoomName
{
    public const int MaxLength = 100;

    public string Value { get; }

    public RoomName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidRoomNameException();
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            throw new InvalidRoomNameException();
        }

        Value = trimmed;
    }

    public override string ToString() => Value;
}
