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
    public const string BookingPriceInvalid = "booking.price_invalid";
    public const string BookingCurrencyInvalid = "booking.currency_invalid";
    public const string BookingExchangeRateInvalid = "booking.exchange_rate_invalid";
    public const string BookingRateProviderInvalid = "booking.rate_provider_invalid";
    public const string BookingRateCapturedAtInvalid = "booking.rate_captured_at_invalid";
    public const string BookingPaymentExpiryInvalid = "booking.payment_expiry_invalid";
    public const string BookingClientRequestInvalid = "booking.client_request_invalid";
    public const string BookingNotPendingPayment = "booking.not_pending_payment";

    public const string PaymentInvalid = "payment.invalid";
    public const string PaymentInvalidState = "payment.invalid_state";
    public const string PaymentNotFound = "payment.not_found";
    public const string PaymentAttemptsExceeded = "payment.attempts_exceeded";
    public const string PaymentNotPending = "payment.not_pending";
    public const string PaymentExpired = "payment.expired";
    public const string PaymentWebhookInvalid = "payment.invalid_webhook";

    public const string RatesUnavailable = "rates.unavailable";
    public const string IdempotencyKeyReused = "idempotency.key_reused";
}
