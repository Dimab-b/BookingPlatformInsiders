namespace BookingPlatformInsiders.Application.Payments.Abstractions;

public sealed record PaymentInitiationRequest(
    Guid BookingId,
    decimal AmountEur,
    string Currency,
    string IdempotencyKey,
    string Description);

public sealed record PaymentInitiationResult(
    string ProviderPaymentId,
    string PaymentUrl);

public sealed record PaymentWebhookNotification(
    string ProviderPaymentId,
    bool IsSucceeded,
    decimal Amount,
    string Currency,
    string? FailureReason,
    string? EventId);

public interface IPaymentProvider
{
    string Name { get; }

    Task<PaymentInitiationResult> CreatePaymentAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default);

    PaymentWebhookNotification? VerifyAndParseWebhook(
        string rawBody,
        IReadOnlyDictionary<string, string> headers);
}
