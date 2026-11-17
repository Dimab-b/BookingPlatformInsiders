using System.Text;
using BookingPlatformInsiders.Application.Auth.Abstractions;
using BookingPlatformInsiders.Application.Common.Time;
using BookingPlatformInsiders.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BookingPlatformInsiders.Infrastructure.Auth;

internal sealed class JwtAccessTokenService : IAccessTokenService
{
    private const string TokenType = "access";
    private const int MinimumSigningKeyBytes = 32;

    private static readonly JsonWebTokenHandler TokenHandler = new();

    private readonly JwtOptions _options;
    private readonly IClock _clock;
    private readonly SigningCredentials _signingCredentials;

    public JwtAccessTokenService(IOptions<JwtOptions> options, IClock clock)
    {
        _options = options.Value;
        _clock = clock;

        if (string.IsNullOrWhiteSpace(_options.Issuer)
            || string.IsNullOrWhiteSpace(_options.Audience))
        {
            throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience must be configured.");
        }

        if (_options.AccessTokenMinutes <= 0)
        {
            throw new InvalidOperationException("Jwt:AccessTokenMinutes must be greater than zero.");
        }

        var signingKeyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);

        if (signingKeyBytes.Length < MinimumSigningKeyBytes)
        {
            throw new InvalidOperationException(
                $"Jwt:SigningKey must be at least {MinimumSigningKeyBytes} bytes long.");
        }

        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(signingKeyBytes),
            SecurityAlgorithms.HmacSha256);
    }

    public AccessToken CreateAccessToken(User user)
    {
        var issuedAtUtc = _clock.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddMinutes(_options.AccessTokenMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = issuedAtUtc,
            NotBefore = issuedAtUtc,
            Expires = expiresAtUtc,
            SigningCredentials = _signingCredentials,
            Claims = new Dictionary<string, object>
            {
                ["sub"] = user.Id.ToString(),
                ["email"] = user.Email.Value,
                ["role"] = user.Role.ToString(),
                ["jti"] = Guid.NewGuid().ToString(),
                ["token_type"] = TokenType
            }
        };

        var token = TokenHandler.CreateToken(descriptor);
        var expiresInSeconds = (int)(expiresAtUtc - issuedAtUtc).TotalSeconds;

        return new AccessToken(token, expiresInSeconds);
    }
}
