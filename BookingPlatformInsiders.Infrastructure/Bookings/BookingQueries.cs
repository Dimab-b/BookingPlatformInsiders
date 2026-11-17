using BookingPlatformInsiders.Application.Bookings;
using BookingPlatformInsiders.Application.Bookings.Dtos;
using BookingPlatformInsiders.Application.Common.Data;
using Dapper;

namespace BookingPlatformInsiders.Infrastructure.Bookings;

internal sealed class BookingQueries : IBookingQueries
{
    private const string WhereClause = """
        "RoomId" = @RoomId
        AND "Status" IN ('Scheduled', 'PendingPayment', 'Completed')
        AND "StartUtc" < @DayEndUtc
        AND "EndUtc" > @DayStartUtc
        """;

    private const string CountSql = $"""
        SELECT COUNT(*)
        FROM "Bookings"
        WHERE {WhereClause};
        """;

    private const string ItemsSql = $"""
        SELECT "Id", "RoomId", "Title", "StartUtc", "EndUtc", "Status",
               ("OwnerId" = @CurrentUserId) AS "IsOwnedByCurrentUser"
        FROM "Bookings"
        WHERE {WhereClause}
        ORDER BY "StartUtc", "Id"
        LIMIT @Limit OFFSET @Offset;
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public BookingQueries(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(IReadOnlyList<BookingDto> Items, int TotalCount)> GetSchedulePageAsync(
        Guid roomId,
        Guid currentUserId,
        DateTime dayStartUtc,
        DateTime dayEndUtc,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var totalCount = await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(
                CountSql,
                new { RoomId = roomId, CurrentUserId = currentUserId, DayStartUtc = dayStartUtc, DayEndUtc = dayEndUtc },
                cancellationToken: cancellationToken));

        if (totalCount == 0)
        {
            return (Array.Empty<BookingDto>(), 0);
        }

        var items = await connection.QueryAsync<BookingDto>(
            new CommandDefinition(
                ItemsSql,
                new
                {
                    RoomId = roomId,
                    CurrentUserId = currentUserId,
                    DayStartUtc = dayStartUtc,
                    DayEndUtc = dayEndUtc,
                    Limit = limit,
                    Offset = offset
                },
                cancellationToken: cancellationToken));

        return (items.AsList(), (int)Math.Min(totalCount, int.MaxValue));
    }
}
