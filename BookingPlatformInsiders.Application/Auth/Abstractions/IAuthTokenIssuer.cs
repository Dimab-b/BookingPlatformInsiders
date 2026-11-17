using BookingPlatformInsiders.Application.Auth.Dtos;
using BookingPlatformInsiders.Application.Common.Time;
using BookingPlatformInsiders.Domain.Users;
using Microsoft.Extensions.Options;

namespace BookingPlatformInsiders.Application.Auth.Abstractions;

public sealed record IssuedTokens(AuthResult AuthResult, RefreshToken RefreshToken);

public interface IAuthTokenIssuer
{
    IssuedTokens Issue(User user, Guid? familyId = null);
}

internal sealed class AuthTokenIssuer : IAuthTokenIssuer
{
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IClock _clock;
    private readonly JwtOptions _options;

    public AuthTokenIssuer(
        IAccessTokenService accessTokenService,
        IRefreshTokenGenerator refreshTokenGenerator,
        IClock clock,
        IOptions<JwtOptions> options)
    {
        if (options.Value.RefreshTokenDays <= 0)
        {
            throw new InvalidOperationException("Jwt:RefreshTokenDays must be greater than zero.");
        }

        _accessTokenService = accessTokenService;
        _refreshTokenGenerator = refreshTokenGenerator;
        _clock = clock;
        _options = options.Value;
    }

    public IssuedTokens Issue(User user, Guid? familyId = null)
    {
        var nowUtc = _clock.UtcNow;

        var accessToken = _accessTokenService.CreateAccessToken(user);
        var rawRefreshToken = _refreshTokenGenerator.Generate();

        var refreshToken = RefreshToken.Create(
            user.Id,
            familyId ?? Guid.NewGuid(),
            _refreshTokenGenerator.Hash(rawRefreshToken),
            nowUtc.AddDays(_options.RefreshTokenDays),
            nowUtc);

        var authResult = new AuthResult(
            accessToken.Token,
            accessToken.ExpiresInSeconds,
            rawRefreshToken,
            new UserDto(user.Id, user.Email.Value, user.Role.ToString()));

        return new IssuedTokens(authResult, refreshToken);
    }
}
