using System.Security.Cryptography;
using System.Text;
using BookingPlatformInsiders.Application.Auth.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace BookingPlatformInsiders.Infrastructure.Auth;

internal sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenBytes = 32;

    public string Generate()
    {
        return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(TokenBytes));
    }

    public string Hash(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.", nameof(refreshToken));
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));

        return Convert.ToHexString(hash);
    }
}
