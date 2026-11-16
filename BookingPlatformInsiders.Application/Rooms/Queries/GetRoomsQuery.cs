using BookingPlatformInsiders.Application.Common.Pagination;
using BookingPlatformInsiders.Application.Rooms.Dtos;
using MediatR;

namespace BookingPlatformInsiders.Application.Rooms.Queries;

public sealed record GetRoomsQuery(int PageNumber = 1, int PageSize = 50)
    : IRequest<PagedResult<RoomDto>>;

public sealed class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, PagedResult<RoomDto>>
{
    private readonly IRoomQueries _roomQueries;

    public GetRoomsQueryHandler(IRoomQueries roomQueries)
    {
        _roomQueries = roomQueries;
    }

    public async Task<PagedResult<RoomDto>> Handle(
        GetRoomsQuery query,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize, offset) = PaginationNormalizer.Normalize(query.PageNumber, query.PageSize);

        var (items, totalCount) = await _roomQueries.GetPageAsync(offset, pageSize, cancellationToken);

        return new PagedResult<RoomDto>(items, totalCount, pageNumber, pageSize);
    }
}
