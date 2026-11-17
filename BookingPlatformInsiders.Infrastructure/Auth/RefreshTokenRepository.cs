using BookingPlatformInsiders.Application.Auth.Abstractions;
using BookingPlatformInsiders.Domain.Users;
using BookingPlatformInsiders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatformInsiders.Infrastructure.Auth;

internal sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<RefreshToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _context.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task RevokeFamilyAsync(
        Guid familyId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens
            .Where(token => token.FamilyId == familyId && token.RevokedAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(token => token.RevokedAtUtc, nowUtc),
                cancellationToken);
    }

    public async Task RevokeAllForUserAsync(
        Guid userId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(token => token.RevokedAtUtc, nowUtc),
                cancellationToken);
    }
}
