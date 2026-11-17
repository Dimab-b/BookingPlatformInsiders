using BookingPlatformInsiders.Application.Payments;
using BookingPlatformInsiders.Domain.Payments;
using BookingPlatformInsiders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatformInsiders.Infrastructure.Payments;

internal sealed class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return _context.Payments.FirstOrDefaultAsync(payment => payment.Id == paymentId, cancellationToken);
    }

    public Task<Payment?> GetByProviderPaymentIdAsync(
        string provider,
        string providerPaymentId,
        CancellationToken cancellationToken = default)
    {
        return _context.Payments.FirstOrDefaultAsync(
            payment => payment.Provider == provider
                && payment.ProviderPaymentId == providerPaymentId,
            cancellationToken);
    }

    public Task<Payment?> GetLatestForBookingAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return _context.Payments
            .AsNoTracking()
            .Where(payment => payment.BookingId == bookingId)
            .OrderByDescending(payment => payment.AttemptNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Payment?> GetActiveInitiatedAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return _context.Payments.FirstOrDefaultAsync(
            payment => payment.BookingId == bookingId
                && payment.Status == PaymentStatus.Initiated,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetAttemptsAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(payment => payment.BookingId == bookingId)
            .OrderBy(payment => payment.AttemptNumber)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAttemptsAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return _context.Payments.CountAsync(
            payment => payment.BookingId == bookingId,
            cancellationToken);
    }

    public Task<bool> HasSucceededAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return _context.Payments.AnyAsync(
            payment => payment.BookingId == bookingId
                && payment.Status == PaymentStatus.Succeeded,
            cancellationToken);
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(payment, cancellationToken);
    }
}
