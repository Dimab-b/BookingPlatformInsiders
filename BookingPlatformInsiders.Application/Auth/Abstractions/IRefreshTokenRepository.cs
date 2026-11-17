using BookingPlatformInsiders.Domain.Users;

namespace BookingPlatformInsiders.Application.Auth.Abstractions;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task RevokeFamilyAsync(Guid familyId, DateTime nowUtc, CancellationToken cancellationToken = default);

    Task RevokeAllForUserAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken = default);
}
