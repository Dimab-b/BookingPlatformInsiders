using BookingPlatformInsiders.Application.Common.Auth;
using BookingPlatformInsiders.Application.Common.Exceptions;
using BookingPlatformInsiders.Application.Common.Persistence;
using BookingPlatformInsiders.Application.Bookings;
using MediatR;

namespace BookingPlatformInsiders.Application.Bookings.Commands;

public sealed record CancelBookingCommand(Guid BookingId) : IRequest;

public sealed class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CancelBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(CancelBookingCommand command, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(command.BookingId, cancellationToken)
            ?? throw new BookingNotFoundException();

        if (booking.OwnerId != _currentUser.UserId)
        {
            throw new BookingNotOwnedException();
        }

        booking.Cancel();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
