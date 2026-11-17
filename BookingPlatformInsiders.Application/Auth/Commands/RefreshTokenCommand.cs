using BookingPlatformInsiders.Application.Auth.Dtos;
using BookingPlatformInsiders.Application.Auth.Abstractions;
using BookingPlatformInsiders.Application.Common.Exceptions;
using BookingPlatformInsiders.Application.Common.Persistence;
using BookingPlatformInsiders.Application.Common.Time;
using BookingPlatformInsiders.Application.Users;
using BookingPlatformInsiders.Domain.Users;
using MediatR;

namespace BookingPlatformInsiders.Application.Auth.Commands;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResult>;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IAuthTokenIssuer _authTokenIssuer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenGenerator refreshTokenGenerator,
        IAuthTokenIssuer authTokenIssuer,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenGenerator = refreshTokenGenerator;
        _authTokenIssuer = authTokenIssuer;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<AuthResult> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            throw new InvalidRefreshTokenRequestException();
        }

        var nowUtc = _clock.UtcNow;
        var tokenHash = _refreshTokenGenerator.Hash(command.RefreshToken);
        var existing = await _refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);

        if (existing is null)
        {
            throw new InvalidRefreshTokenRequestException();
        }

        if (existing.RevokedAtUtc is not null)
        {
            await _refreshTokenRepository.RevokeFamilyAsync(existing.FamilyId, nowUtc, cancellationToken);
            throw new InvalidRefreshTokenRequestException();
        }

        if (!existing.IsActive(nowUtc))
        {
            throw new RefreshTokenExpiredException();
        }

        var user = await _userRepository.GetByIdAsync(existing.UserId, cancellationToken)
            ?? throw new InvalidRefreshTokenRequestException();

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                var issued = _authTokenIssuer.Issue(user, existing.FamilyId);

                existing.Rotate(issued.RefreshToken.Id, nowUtc);

                await _refreshTokenRepository.AddAsync(issued.RefreshToken, token);
                await _unitOfWork.SaveChangesAsync(token);

                return issued.AuthResult;
            }, cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            await _refreshTokenRepository.RevokeFamilyAsync(existing.FamilyId, nowUtc, cancellationToken);
            throw new InvalidRefreshTokenRequestException();
        }
    }
}
