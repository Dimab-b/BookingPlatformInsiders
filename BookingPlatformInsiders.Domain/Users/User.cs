using BookingPlatformInsiders.Domain.Common;

namespace BookingPlatformInsiders.Domain.Users;

public sealed class User : Entity, IAggregateRoot
{
    public const int MaxPasswordHashLength = 200;

    public Guid Id { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private User()
    {
        Email = null!;
        PasswordHash = null!;
    }

    private User(
        Guid id,
        Email email,
        string passwordHash,
        UserRole role,
        DateTime createdAtUtc)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAtUtc = createdAtUtc;
    }

    public static User Create(
        Email email,
        string passwordHash,
        UserRole role,
        DateTime createdAtUtc)
    {
        if (email is null)
        {
            throw new InvalidEmailException();
        }

        if (string.IsNullOrWhiteSpace(passwordHash)
            || passwordHash.Length > MaxPasswordHashLength)
        {
            throw new InvalidPasswordHashException();
        }

        if (!Enum.IsDefined(role))
        {
            throw new InvalidUserRoleException();
        }

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidUserCreatedAtException();
        }

        return new User(Guid.NewGuid(), email, passwordHash, role, createdAtUtc);
    }

    public override bool Equals(object? obj) =>
        obj is User other && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}
