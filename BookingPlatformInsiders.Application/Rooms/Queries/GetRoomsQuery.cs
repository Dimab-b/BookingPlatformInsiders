using BookingPlatformInsiders.Application.Common.Pagination;
using BookingPlatformInsiders.Application.Rooms.Dtos;
using MediatR;

namespace BookingPlatformInsiders.Application.Rooms.Queries;

public sealed record GetRoomsQuery(int PageNumber = 1, int PageSize = 50)
    : IRequest<PagedResult<RoomDto>>;

public sealed class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, PagedResult<RoomDto>>
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 100;

    private readonly IRoomQueries _roomQueries;

    public GetRoomsQueryHandler(IRoomQueries roomQueries)
    {
        _roomQueries = roomQueries;
    }

    public async Task<PagedResult<RoomDto>> Handle(
        GetRoomsQuery query,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize <= 0 ? DefaultPageSize : query.PageSize, 1, MaxPageSize);

        var offsetInLong = (long)(pageNumber - 1) * pageSize;
        var offset = offsetInLong > int.MaxValue ? int.MaxValue : (int)offsetInLong;

        var (items, totalCount) = await _roomQueries.GetPageAsync(offset, pageSize, cancellationToken);

        return new PagedResult<RoomDto>(items, totalCount, pageNumber, pageSize);
    }
}
