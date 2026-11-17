using BookingPlatformInsiders.Application.Auth.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace BookingPlatformInsiders.Infrastructure.Auth;

internal sealed class PasswordHasherAdapter : IPasswordHasher
{
    private static readonly object HasherUser = new();
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(HasherUser, password);
    }

    public bool Verify(string passwordHash, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(HasherUser, passwordHash, password);

        return result != PasswordVerificationResult.Failed;
    }
}
