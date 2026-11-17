using BookingPlatformInsiders.Domain.Payments;

namespace BookingPlatformInsiders.Application.Payments;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);

    Task<Payment?> GetByProviderPaymentIdAsync(
        string provider,
        string providerPaymentId,
        CancellationToken cancellationToken = default);

    Task<Payment?> GetLatestForBookingAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<Payment?> GetActiveInitiatedAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Payment>> GetAttemptsAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<int> CountAttemptsAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<bool> HasSucceededAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}
