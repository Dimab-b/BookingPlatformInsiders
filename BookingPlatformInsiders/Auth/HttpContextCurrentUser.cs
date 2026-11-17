using BookingPlatformInsiders.Application.Common.Auth;

namespace BookingPlatformInsiders.Auth;

internal sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId =>
        Guid.TryParse(FindClaim("sub"), out var userId) ? userId : Guid.Empty;

    public string Email => FindClaim("email") ?? string.Empty;

    public string Role => FindClaim("role") ?? string.Empty;

    private string? FindClaim(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(claimType)?.Value;
    }
}
