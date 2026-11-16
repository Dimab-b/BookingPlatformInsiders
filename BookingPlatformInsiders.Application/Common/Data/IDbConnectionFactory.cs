using System.Data.Common;

namespace BookingPlatformInsiders.Application.Common.Data;

public interface IDbConnectionFactory
{
    Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
