using BookingPlatformInsiders.Application.Auth.Abstractions;
using BookingPlatformInsiders.Application.Common.Time;
using MediatR;

namespace BookingPlatformInsiders.Application.Auth.Commands;

public sealed record LogoutCommand(string RefreshToken) : IRequest;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IClock _clock;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenGenerator refreshTokenGenerator,
        IClock clock)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenGenerator = refreshTokenGenerator;
        _clock = clock;
    }

    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return;
        }

        var tokenHash = _refreshTokenGenerator.Hash(command.RefreshToken);
        var existing = await _refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);

        if (existing is null)
        {
            return;
        }

        await _refreshTokenRepository.RevokeFamilyAsync(existing.FamilyId, _clock.UtcNow, cancellationToken);
    }
}
