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

    public const string UserEmailInvalid = "user.email_invalid";
    public const string UserPasswordHashInvalid = "user.password_hash_invalid";
    public const string UserRoleInvalid = "user.role_invalid";
    public const string UserCreatedAtInvalid = "user.created_at_invalid";

    public const string AuthInvalidCredentials = "auth.invalid_credentials";
    public const string AuthEmailTaken = "auth.email_taken";
    public const string AuthInvalidToken = "auth.invalid_token";
    public const string AuthSessionExpired = "auth.session_expired";

    public const string BookingOwnerInvalid = "booking.owner_invalid";
    public const string BookingNotOwned = "booking.not_owned";
}
