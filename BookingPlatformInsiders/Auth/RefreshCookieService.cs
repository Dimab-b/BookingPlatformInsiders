using BookingPlatformInsiders.Application.Auth.Abstractions;
using BookingPlatformInsiders.Configuration;
using Microsoft.Extensions.Options;

namespace BookingPlatformInsiders.Auth;

public sealed class RefreshCookieService
{
    private readonly RefreshCookieOptions _options;
    private readonly int _refreshTokenDays;

    public RefreshCookieService(
        IOptions<RefreshCookieOptions> options,
        IOptions<JwtOptions> jwtOptions)
    {
        _options = options.Value;

        if (jwtOptions.Value.RefreshTokenDays <= 0)
        {
            throw new InvalidOperationException("Jwt:RefreshTokenDays must be greater than zero.");
        }

        _refreshTokenDays = jwtOptions.Value.RefreshTokenDays;
    }

    public string? Read(HttpRequest request)
    {
        return request.Cookies.TryGetValue(_options.RefreshCookieName, out var value)
            ? value
            : null;
    }

    public void Set(HttpResponse response, string refreshToken)
    {
        var cookieOptions = CreateCookieOptions(DateTimeOffset.UtcNow.AddDays(_refreshTokenDays));

        response.Cookies.Append(_options.RefreshCookieName, refreshToken, cookieOptions);
    }

    public void Clear(HttpResponse response)
    {
        var cookieOptions = CreateCookieOptions(DateTimeOffset.UnixEpoch);

        response.Cookies.Append(_options.RefreshCookieName, string.Empty, cookieOptions);
    }

    private CookieOptions CreateCookieOptions(DateTimeOffset expires)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = _options.RequireSecureCookies,
            SameSite = ParseSameSite(_options.RefreshCookieSameSite),
            Path = _options.RefreshCookiePath,
            Expires = expires
        };
    }

    private static SameSiteMode ParseSameSite(string value)
    {
        return Enum.TryParse<SameSiteMode>(value, ignoreCase: true, out var mode)
            ? mode
            : SameSiteMode.Lax;
    }
}
