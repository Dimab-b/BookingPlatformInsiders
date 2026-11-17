using BookingPlatformInsiders.Application.Auth.Abstractions;
using BookingPlatformInsiders.Application.Common.Auth;
using BookingPlatformInsiders.Application.Common.Time;
using MediatR;

namespace BookingPlatformInsiders.Application.Auth.Commands;

public sealed record LogoutAllCommand : IRequest;

public sealed class LogoutAllCommandHandler : IRequestHandler<LogoutAllCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public LogoutAllCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ICurrentUser currentUser,
        IClock clock)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task Handle(LogoutAllCommand command, CancellationToken cancellationToken)
    {
        await _refreshTokenRepository.RevokeAllForUserAsync(
            _currentUser.UserId,
            _clock.UtcNow,
            cancellationToken);
    }
}
