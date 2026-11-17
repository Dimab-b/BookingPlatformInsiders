namespace BookingPlatformInsiders.Application.Common.Auth;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Email { get; }
    string Role { get; }
}
