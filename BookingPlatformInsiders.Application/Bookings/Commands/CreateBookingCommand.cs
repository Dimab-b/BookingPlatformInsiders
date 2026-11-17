using BookingPlatformInsiders.Application.Bookings.Dtos;
using BookingPlatformInsiders.Application.Common.Auth;
using BookingPlatformInsiders.Application.Common.Exceptions;
using BookingPlatformInsiders.Application.Common.Persistence;
using BookingPlatformInsiders.Application.Common.Time;
using BookingPlatformInsiders.Application.Payments;
using BookingPlatformInsiders.Application.Payments.Abstractions;
using BookingPlatformInsiders.Application.Rooms;
using BookingPlatformInsiders.Domain.Bookings;
using BookingPlatformInsiders.Domain.Payments;
using MediatR;
using Microsoft.Extensions.Options;

namespace BookingPlatformInsiders.Application.Bookings.Commands;

public sealed record CreateBookingCommand(
    Guid RoomId,
    string Title,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    decimal? AmountOriginal = null,
    string? Currency = null,
    string? IdempotencyKey = null) : IRequest<BookingDto>;

public sealed class CreateBookingCommandHandler
    : IRequestHandler<CreateBookingCommand, BookingDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentProviderResolver _paymentProviderResolver;
    private readonly IRateProvider _rateProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly PaymentOptions _paymentOptions;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        IPaymentRepository paymentRepository,
        IPaymentProviderResolver paymentProviderResolver,
        IRateProvider rateProvider,
        IUnitOfWork unitOfWork,
        IClock clock,
        ICurrentUser currentUser,
        IOptions<PaymentOptions> paymentOptions)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _paymentRepository = paymentRepository;
        _paymentProviderResolver = paymentProviderResolver;
        _rateProvider = rateProvider;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUser = currentUser;
        _paymentOptions = paymentOptions.Value;

        if (_paymentOptions.PaymentTtlMinutes <= 0)
        {
            throw new InvalidOperationException("Payments:PaymentTtlMinutes must be greater than zero.");
        }
    }

    public async Task<BookingDto> Handle(
        CreateBookingCommand command,
        CancellationToken cancellationToken)
    {
        var startUtc = command.StartUtc.UtcDateTime;
        var endUtc = command.EndUtc.UtcDateTime;
        var nowUtc = _clock.UtcNow;
        var clientRequestId = NormalizeClientRequestId(command.IdempotencyKey);

        if (clientRequestId is not null)
        {
            var existing = await _bookingRepository.GetByClientRequestIdAsync(
                _currentUser.UserId,
                clientRequestId,
                cancellationToken);

            if (existing is not null)
            {
                return Map(existing);
            }
        }

        var room = await _roomRepository.GetByIdAsync(command.RoomId, cancellationToken)
            ?? throw new RoomNotFoundException();

        BookingPrice? price = null;

        if (room.IsPaid)
        {
            price = await BuildPriceAsync(command, nowUtc, cancellationToken);
        }

        var booking = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var candidates = await _bookingRepository.GetOverlapCandidatesAsync(
                command.RoomId,
                startUtc,
                endUtc,
                token);

            Booking.EnsureNoOverlap(candidates, command.RoomId, startUtc, endUtc);

            if (!room.IsPaid)
            {
                var freeBooking = Booking.Create(
                    _currentUser.UserId,
                    command.RoomId,
                    new BookingTitle(command.Title),
                    startUtc,
                    endUtc,
                    nowUtc,
                    clientRequestId);

                await _bookingRepository.AddAsync(freeBooking, token);
                await _unitOfWork.SaveChangesAsync(token);

                return freeBooking;
            }

            var paymentExpiresAtUtc = nowUtc.AddMinutes(_paymentOptions.PaymentTtlMinutes);

            var pendingBooking = Booking.CreatePendingPayment(
                _currentUser.UserId,
                command.RoomId,
                new BookingTitle(command.Title),
                startUtc,
                endUtc,
                nowUtc,
                price!,
                paymentExpiresAtUtc,
                clientRequestId);

            var payment = Payment.CreateInitiated(
                pendingBooking.Id,
                _paymentProviderResolver.Current.Name,
                $"{pendingBooking.Id:N}:p1",
                1,
                nowUtc);

            await _bookingRepository.AddAsync(pendingBooking, token);
            await _paymentRepository.AddAsync(payment, token);
            await _unitOfWork.SaveChangesAsync(token);

            return pendingBooking;
        }, cancellationToken);

        return Map(booking);
    }

    private async Task<BookingPrice> BuildPriceAsync(
        CreateBookingCommand command,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        if (command.AmountOriginal is null || command.AmountOriginal <= 0)
        {
            throw new InvalidBookingPriceException();
        }

        if (string.IsNullOrWhiteSpace(command.Currency))
        {
            throw new InvalidBookingCurrencyException();
        }

        var currency = command.Currency.Trim().ToUpperInvariant();
        var quote = await _rateProvider.GetEurRateAsync(currency, cancellationToken);
        var amountEur = decimal.Round(
            command.AmountOriginal.Value * quote.EurPerUnit,
            2,
            MidpointRounding.AwayFromZero);

        return new BookingPrice(
            command.AmountOriginal.Value,
            currency,
            amountEur,
            quote.EurPerUnit,
            quote.Provider,
            quote.CapturedAtUtc);
    }

    private BookingDto Map(Booking booking) =>
        new(
            booking.Id,
            booking.RoomId,
            booking.Title.Value,
            booking.StartUtc,
            booking.EndUtc,
            booking.Status.ToString(),
            booking.OwnerId == _currentUser.UserId,
            booking.Price?.AmountOriginal,
            booking.Price?.Currency,
            booking.Price?.AmountEur,
            booking.PaymentExpiresAtUtc);

    private static string? NormalizeClientRequestId(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
