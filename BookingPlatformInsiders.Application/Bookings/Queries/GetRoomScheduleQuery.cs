using BookingPlatformInsiders.Application.Bookings;
using BookingPlatformInsiders.Application.Bookings.Dtos;
using BookingPlatformInsiders.Application.Common.Exceptions;
using BookingPlatformInsiders.Application.Common.Pagination;
using BookingPlatformInsiders.Application.Rooms;
using MediatR;

namespace BookingPlatformInsiders.Application.Bookings.Queries;

public sealed record GetRoomScheduleQuery(
    Guid RoomId,
    DateOnly Date,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PagedResult<BookingDto>>;

public sealed class GetRoomScheduleQueryHandler
    : IRequestHandler<GetRoomScheduleQuery, PagedResult<BookingDto>>
{
    private readonly IBookingQueries _bookingQueries;
    private readonly IRoomRepository _roomRepository;

    public GetRoomScheduleQueryHandler(
        IBookingQueries bookingQueries,
        IRoomRepository roomRepository)
    {
        _bookingQueries = bookingQueries;
        _roomRepository = roomRepository;
    }

    public async Task<PagedResult<BookingDto>> Handle(
        GetRoomScheduleQuery query,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize, offset) = PaginationNormalizer.Normalize(query.PageNumber, query.PageSize);

        var roomExists = await _roomRepository.ExistsAsync(query.RoomId, cancellationToken);

        if (!roomExists)
        {
            throw new RoomNotFoundException();
        }

        var dayStartUtc = query.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEndUtc = dayStartUtc.AddDays(1);

        var (items, totalCount) = await _bookingQueries.GetSchedulePageAsync(
            query.RoomId,
            dayStartUtc,
            dayEndUtc,
            offset,
            pageSize,
            cancellationToken);

        return new PagedResult<BookingDto>(items, totalCount, pageNumber, pageSize);
    }
}
