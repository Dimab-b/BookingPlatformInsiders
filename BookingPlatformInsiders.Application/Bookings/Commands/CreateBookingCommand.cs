using BookingPlatformInsiders.Application.Bookings.Dtos;
using BookingPlatformInsiders.Application.Common.Auth;
using BookingPlatformInsiders.Application.Common.Exceptions;
using BookingPlatformInsiders.Application.Common.Persistence;
using BookingPlatformInsiders.Application.Common.Time;
using BookingPlatformInsiders.Application.Rooms;
using BookingPlatformInsiders.Domain.Bookings;
using MediatR;

namespace BookingPlatformInsiders.Application.Bookings.Commands;

public sealed record CreateBookingCommand(
    Guid RoomId,
    string Title,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc) : IRequest<BookingDto>;

public sealed class CreateBookingCommandHandler
    : IRequestHandler<CreateBookingCommand, BookingDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        IUnitOfWork unitOfWork,
        IClock clock,
        ICurrentUser currentUser)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<BookingDto> Handle(
        CreateBookingCommand command,
        CancellationToken cancellationToken)
    {
        var startUtc = command.StartUtc.UtcDateTime;
        var endUtc = command.EndUtc.UtcDateTime;
        var nowUtc = _clock.UtcNow;

        var booking = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var roomExists = await _roomRepository.ExistsAsync(command.RoomId, token);

            if (!roomExists)
            {
                throw new RoomNotFoundException();
            }

            var candidates = await _bookingRepository.GetOverlapCandidatesAsync(
                command.RoomId,
                startUtc,
                endUtc,
                token);

            Booking.EnsureNoOverlap(candidates, command.RoomId, startUtc, endUtc);

            var created = Booking.Create(
                _currentUser.UserId,
                command.RoomId,
                new BookingTitle(command.Title),
                startUtc,
                endUtc,
                nowUtc);

            await _bookingRepository.AddAsync(created, token);
            await _unitOfWork.SaveChangesAsync(token);

            return created;
        }, cancellationToken);

        return Map(booking);
    }

    private BookingDto Map(Booking booking) =>
        new(
            booking.Id,
            booking.RoomId,
            booking.Title.Value,
            booking.StartUtc,
            booking.EndUtc,
            booking.Status.ToString(),
            booking.OwnerId == _currentUser.UserId);
}
