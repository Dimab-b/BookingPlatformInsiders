using BookingPlatformInsiders.Application.Common.Data;
using BookingPlatformInsiders.Application.Rooms;
using BookingPlatformInsiders.Application.Rooms.Dtos;
using Dapper;

namespace BookingPlatformInsiders.Infrastructure.Rooms;

internal sealed class RoomQueries : IRoomQueries
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoomQueries(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(IReadOnlyList<RoomDto> Items, int TotalCount)> GetPageAsync(
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        const string countSql = """
            SELECT COUNT(*)
            FROM "Rooms";
            """;

        const string itemsSql = """
            SELECT "Id", "Name", "Capacity", "Location", "Description", "IsPaid"
            FROM "Rooms"
            ORDER BY lower("Name"), "Id"
            LIMIT @Limit OFFSET @Offset;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var totalCount = await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(countSql, cancellationToken: cancellationToken));

        if (totalCount == 0)
        {
            return (Array.Empty<RoomDto>(), 0);
        }

        var items = await connection.QueryAsync<RoomDto>(
            new CommandDefinition(
                itemsSql,
                new { Limit = limit, Offset = offset },
                cancellationToken: cancellationToken));

        return (items.AsList(), (int)Math.Min(totalCount, int.MaxValue));
    }
}
