using BookingPlatformInsiders.Application.Bookings;
using BookingPlatformInsiders.Application.Common.Auth;
using BookingPlatformInsiders.Application.Common.Exceptions;
using BookingPlatformInsiders.Application.Common.Persistence;
using BookingPlatformInsiders.Application.Common.Time;
using BookingPlatformInsiders.Application.Payments.Abstractions;
using BookingPlatformInsiders.Application.Payments.Dtos;
using BookingPlatformInsiders.Domain.Bookings;
using BookingPlatformInsiders.Domain.Payments;
using MediatR;
using Microsoft.Extensions.Options;

namespace BookingPlatformInsiders.Application.Payments.Commands;

public sealed record InitiatePaymentCommand(
    Guid BookingId,
    string? IdempotencyKey = null) : IRequest<InitiatePaymentResult>;

public sealed class InitiatePaymentCommandHandler
    : IRequestHandler<InitiatePaymentCommand, InitiatePaymentResult>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentProviderResolver _paymentProviderResolver;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly PaymentOptions _paymentOptions;

    public InitiatePaymentCommandHandler(
        IBookingRepository bookingRepository,
        IPaymentRepository paymentRepository,
        IPaymentProviderResolver paymentProviderResolver,
        IUnitOfWork unitOfWork,
        IClock clock,
        ICurrentUser currentUser,
        IOptions<PaymentOptions> paymentOptions)
    {
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _paymentProviderResolver = paymentProviderResolver;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUser = currentUser;
        _paymentOptions = paymentOptions.Value;

        if (_paymentOptions.MaxAttempts <= 0)
        {
            throw new InvalidOperationException("Payments:MaxAttempts must be greater than zero.");
        }
    }

    public async Task<InitiatePaymentResult> Handle(
        InitiatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(command.BookingId, cancellationToken)
            ?? throw new BookingNotFoundException();

        if (booking.OwnerId != _currentUser.UserId)
        {
            throw new BookingNotOwnedException();
        }

        var nowUtc = _clock.UtcNow;

        if (booking.Status != BookingStatus.PendingPayment)
        {
            throw new PaymentNotPendingException();
        }

        if (booking.PaymentExpiresAtUtc is null || nowUtc >= booking.PaymentExpiresAtUtc.Value)
        {
            throw new PaymentExpiredException();
        }

        var attemptCount = await _paymentRepository.CountAttemptsAsync(booking.Id, cancellationToken);
        var activePayment = await _paymentRepository.GetActiveInitiatedAsync(booking.Id, cancellationToken);

        if (activePayment is null && attemptCount >= _paymentOptions.MaxAttempts)
        {
            throw new PaymentAttemptsExceededException();
        }

        var payment = activePayment ?? Payment.CreateInitiated(
            booking.Id,
            _paymentProviderResolver.Current.Name,
            NormalizeIdempotencyKey(command.IdempotencyKey, booking.Id, attemptCount + 1),
            attemptCount + 1,
            nowUtc);

        if (activePayment is null)
        {
            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        try
        {
            var providerResult = await _paymentProviderResolver.Current.CreatePaymentAsync(
                new PaymentInitiationRequest(
                    booking.Id,
                    booking.Price!.AmountEur,
                    "EUR",
                    payment.IdempotencyKey,
                    booking.Title.Value),
                cancellationToken);

            payment.AttachProviderPayment(providerResult.ProviderPaymentId, nowUtc);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new InitiatePaymentResult(
                payment.Id,
                booking.Id,
                payment.AttemptNumber,
                payment.Provider,
                payment.Status.ToString(),
                providerResult.PaymentUrl,
                booking.PaymentExpiresAtUtc.Value);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            payment.Fail("Payment provider call failed.", _clock.UtcNow);

            if (payment.AttemptNumber >= _paymentOptions.MaxAttempts)
            {
                booking.MarkPaymentFailed();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    private static string NormalizeIdempotencyKey(string? value, Guid bookingId, int attemptNumber)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }

        return $"{bookingId:N}:p{attemptNumber}";
    }
}
