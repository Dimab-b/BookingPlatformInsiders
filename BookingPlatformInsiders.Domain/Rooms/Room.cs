using BookingPlatformInsiders.Domain.Common;

namespace BookingPlatformInsiders.Domain.Rooms;

public sealed class Room : Entity, IAggregateRoot
{
    public const int MinCapacity = 1;
    public const int MaxCapacity = 100;
    public const int MaxLocationLength = 100;
    public const int MaxDescriptionLength = 500;

    public Guid Id { get; private set; }
    public RoomName Name { get; private set; }
    public int Capacity { get; private set; }
    public string Location { get; private set; }
    public string? Description { get; private set; }
    public bool IsPaid { get; private set; }

    private Room()
    {
        Name = null!;
        Location = null!;
    }

    private Room(
        Guid id,
        RoomName name,
        int capacity,
        string location,
        string? description,
        bool isPaid)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
        Location = location;
        Description = description;
        IsPaid = isPaid;
    }

    public static Room Create(
        RoomName name,
        int capacity,
        string location,
        string? description = null,
        bool isPaid = false)
    {
        if (capacity < MinCapacity || capacity > MaxCapacity)
        {
            throw new InvalidRoomCapacityException();
        }


        return new Room(
            Guid.NewGuid(),
            name,
            capacity,
            NormalizeLocation(location),
            NormalizeDescription(description),
            isPaid);
    }

    public void Rename(RoomName name)
    {
        Name = name;
    }

    public override bool Equals(object? obj) =>
        obj is Room other && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();

    private static string NormalizeLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            throw new InvalidRoomLocationException();
        }

        var trimmed = location.Trim();

        if (trimmed.Length > MaxLocationLength)
        {
            throw new InvalidRoomLocationException();
        }

        return trimmed;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var trimmed = description.Trim();

        if (trimmed.Length > MaxDescriptionLength)
        {
            throw new InvalidRoomDescriptionException();
        }

        return trimmed;
    }
}
