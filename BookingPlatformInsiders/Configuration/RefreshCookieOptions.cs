namespace BookingPlatformInsiders.Configuration;

public sealed class RefreshCookieOptions
{
    public const string SectionName = "Auth";

    public string RefreshCookieName { get; set; } = "refresh_token";
    public string RefreshCookiePath { get; set; } = "/api/auth";
    public bool RequireSecureCookies { get; set; }
    public string RefreshCookieSameSite { get; set; } = "Lax";
}
