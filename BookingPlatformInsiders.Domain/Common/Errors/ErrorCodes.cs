namespace BookingPlatformInsiders.Domain.Common.Errors;

public static class ErrorCodes
{
    public const string RoomNameInvalid = "room.name_invalid";
    public const string RoomCapacityInvalid = "room.capacity_invalid";
    public const string RoomLocationInvalid = "room.location_invalid";
    public const string RoomDescriptionInvalid = "room.description_invalid";

    public const string BookingTitleInvalid = "booking.title_invalid";
    public const string BookingRoomInvalid = "booking.room_invalid";
    public const string BookingTimeRangeInvalid = "booking.time_range_invalid";
    public const string BookingTimeNotUtc = "booking.time_not_utc";
    public const string BookingStartInPast = "booking.start_in_past";
    public const string BookingSlotTaken = "booking.slot_taken";
    public const string BookingCannotCancel = "booking.cannot_cancel";
    public const string BookingCannotComplete = "booking.cannot_complete";

    public const string RoomNotFound = "room.not_found";
    public const string BookingNotFound = "booking.not_found";
    public const string ConcurrencyConflict = "concurrency.conflict";
}
