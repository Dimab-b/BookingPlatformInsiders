namespace BookingPlatformInsiders.Infrastructure.Rooms;

internal static class RoomSeedData
{
    internal sealed record SeedRoom(string Name, int Capacity, string Location, string? Description);

    public static IReadOnlyList<SeedRoom> Rooms { get; } =
    [
        new("Aurora", 8, "Floor 1 - East wing", "Boardroom with video conferencing."),
        new("Boreal", 12, "Floor 2 - West wing", null)
    ];
}
