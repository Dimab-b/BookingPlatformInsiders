using BookingPlatformInsiders.Application.Bookings;
using BookingPlatformInsiders.Application.Common.Persistence;
using BookingPlatformInsiders.Domain.Bookings;
using MediatR;

namespace BookingPlatformInsiders.Application.Bookings.Commands;

public enum CancelBookingResult
{
    Cancelled,
    NotFound,
    CannotCancel
}

public sealed record CancelBookingCommand(Guid BookingId) : IRequest<CancelBookingResult>;

public sealed class CancelBookingCommandHandler
    : IRequestHandler<CancelBookingCommand, CancelBookingResult>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CancelBookingResult> Handle(
        CancelBookingCommand command,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);

        if (booking is null)
        {
            return CancelBookingResult.NotFound;
        }

        try
        {
            booking.Cancel();
        }
        catch (BookingCannotCancelException)
        {
            return CancelBookingResult.CannotCancel;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CancelBookingResult.Cancelled;
    }
}
