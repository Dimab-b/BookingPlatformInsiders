using BookingPlatformInsiders.Application.Bookings;
using BookingPlatformInsiders.Application.Common.Persistence;
using BookingPlatformInsiders.Application.Common.Time;
using MediatR;

namespace BookingPlatformInsiders.Application.Bookings.Commands;

public sealed record CompleteExpiredBookingsCommand(int BatchSize, int MaxBatches) : IRequest<int>;

public sealed class CompleteExpiredBookingsCommandHandler
    : IRequestHandler<CompleteExpiredBookingsCommand, int>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CompleteExpiredBookingsCommandHandler(
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<int> Handle(
        CompleteExpiredBookingsCommand command,
        CancellationToken cancellationToken)
    {
        var nowUtc = _clock.UtcNow;
        var completed = 0;

        for (var batchIndex = 0; batchIndex < command.MaxBatches; batchIndex++)
        {
            var expired = await _bookingRepository.GetExpiredScheduledAsync(
                nowUtc,
                command.BatchSize,
                cancellationToken);

            if (expired.Count == 0)
            {
                break;
            }

            foreach (var booking in expired)
            {
                booking.Complete(nowUtc);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            completed += expired.Count;

            if (expired.Count < command.BatchSize)
            {
                break;
            }
        }

        return completed;
    }
}
