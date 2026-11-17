using BookingPlatformInsiders.Domain.Common;

namespace BookingPlatformInsiders.Domain.Payments;

public sealed class Payment : Entity, IAggregateRoot
{
    public const int MaxProviderLength = 50;
    public const int MaxProviderPaymentIdLength = 200;
    public const int MaxIdempotencyKeyLength = 100;
    public const int MaxFailureReasonLength = 500;

    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public string Provider { get; private set; }
    public string? ProviderPaymentId { get; private set; }
    public string IdempotencyKey { get; private set; }
    public int AttemptNumber { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Payment()
    {
        Provider = null!;
        IdempotencyKey = null!;
    }

    private Payment(
        Guid id,
        Guid bookingId,
        string provider,
        string idempotencyKey,
        int attemptNumber,
        PaymentStatus status,
        string? providerPaymentId,
        string? failureReason,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        BookingId = bookingId;
        Provider = provider;
        IdempotencyKey = idempotencyKey;
        AttemptNumber = attemptNumber;
        Status = status;
        ProviderPaymentId = providerPaymentId;
        FailureReason = failureReason;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public static Payment CreateInitiated(
        Guid bookingId,
        string provider,
        string idempotencyKey,
        int attemptNumber,
        DateTime nowUtc)
    {
        if (bookingId == Guid.Empty)
        {
            throw new InvalidPaymentException();
        }

        if (string.IsNullOrWhiteSpace(provider)
            || provider.Trim().Length > MaxProviderLength)
        {
            throw new InvalidPaymentException();
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey)
            || idempotencyKey.Trim().Length > MaxIdempotencyKeyLength)
        {
            throw new InvalidPaymentException();
        }

        if (attemptNumber <= 0)
        {
            throw new InvalidPaymentException();
        }

        EnsureUtc(nowUtc);

        return new Payment(
            Guid.NewGuid(),
            bookingId,
            provider.Trim().ToLowerInvariant(),
            idempotencyKey.Trim(),
            attemptNumber,
            PaymentStatus.Initiated,
            null,
            null,
            nowUtc,
            nowUtc);
    }

    public void AttachProviderPayment(string providerPaymentId, DateTime nowUtc)
    {
        EnsureUtc(nowUtc);

        if (Status != PaymentStatus.Initiated)
        {
            throw new InvalidPaymentStateException(
                "Provider payment can only be attached to an initiated payment.");
        }

        if (string.IsNullOrWhiteSpace(providerPaymentId)
            || providerPaymentId.Trim().Length > MaxProviderPaymentIdLength)
        {
            throw new InvalidPaymentException();
        }

        var normalizedId = providerPaymentId.Trim();

        if (ProviderPaymentId is not null && ProviderPaymentId != normalizedId)
        {
            throw new InvalidPaymentStateException(
                "A different provider payment id is already attached.");
        }

        ProviderPaymentId = normalizedId;
        UpdatedAtUtc = nowUtc;
    }

    public void Succeed(DateTime nowUtc)
    {
        EnsureUtc(nowUtc);

        if (Status == PaymentStatus.Succeeded)
        {
            return;
        }

        if (Status != PaymentStatus.Initiated)
        {
            throw new InvalidPaymentStateException("Only an initiated payment can succeed.");
        }

        if (ProviderPaymentId is null)
        {
            throw new InvalidPaymentStateException(
                "Provider payment id is required before a payment can succeed.");
        }

        Status = PaymentStatus.Succeeded;
        FailureReason = null;
        UpdatedAtUtc = nowUtc;
    }

    public void Fail(string reason, DateTime nowUtc)
    {
        EnsureUtc(nowUtc);

        if (Status == PaymentStatus.Failed)
        {
            return;
        }

        if (Status != PaymentStatus.Initiated)
        {
            throw new InvalidPaymentStateException("Only an initiated payment can fail.");
        }

        Status = PaymentStatus.Failed;
        FailureReason = NormalizeFailureReason(reason);
        UpdatedAtUtc = nowUtc;
    }

    public void Expire(DateTime nowUtc)
    {
        EnsureUtc(nowUtc);

        if (Status == PaymentStatus.Expired)
        {
            return;
        }

        if (Status != PaymentStatus.Initiated)
        {
            throw new InvalidPaymentStateException("Only an initiated payment can expire.");
        }

        Status = PaymentStatus.Expired;
        UpdatedAtUtc = nowUtc;
    }

    public override bool Equals(object? obj) =>
        obj is Payment other && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();

    private static string? NormalizeFailureReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return null;
        }

        var trimmed = reason.Trim();

        if (trimmed.Length > MaxFailureReasonLength)
        {
            throw new InvalidPaymentException();
        }

        return trimmed;
    }

    private static void EnsureUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new InvalidPaymentException();
        }
    }
}
