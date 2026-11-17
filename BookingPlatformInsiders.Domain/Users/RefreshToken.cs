using BookingPlatformInsiders.Domain.Common;

namespace BookingPlatformInsiders.Domain.Users;

public sealed class RefreshToken : Entity, IAggregateRoot
{
    public const int MaxTokenHashLength = 128;

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid FamilyId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    private RefreshToken()
    {
        TokenHash = null!;
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        Guid familyId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        Id = id;
        UserId = userId;
        FamilyId = familyId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static RefreshToken Create(
        Guid userId,
        Guid familyId,
        string tokenHash,
        DateTime expiresAtUtc,
        DateTime nowUtc)
    {
        if (userId == Guid.Empty || familyId == Guid.Empty)
        {
            throw new InvalidRefreshTokenException();
        }

        if (string.IsNullOrWhiteSpace(tokenHash) || tokenHash.Length > MaxTokenHashLength)
        {
            throw new InvalidRefreshTokenException();
        }

        if (nowUtc.Kind != DateTimeKind.Utc || expiresAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidRefreshTokenException();
        }

        if (expiresAtUtc <= nowUtc)
        {
            throw new InvalidRefreshTokenException();
        }

        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            familyId,
            tokenHash,
            nowUtc,
            expiresAtUtc);
    }

    public bool IsActive(DateTime nowUtc) =>
        RevokedAtUtc is null && nowUtc < ExpiresAtUtc;

    public void Revoke(DateTime nowUtc)
    {
        if (nowUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidRefreshTokenException();
        }

        if (RevokedAtUtc is not null)
        {
            throw new InvalidRefreshTokenStateException("The refresh token is already revoked.");
        }

        RevokedAtUtc = nowUtc;
    }

    public void Rotate(Guid successorId, DateTime nowUtc)
    {
        if (successorId == Guid.Empty || nowUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidRefreshTokenException();
        }

        if (RevokedAtUtc is not null)
        {
            throw new InvalidRefreshTokenStateException("The refresh token is already revoked.");
        }

        if (nowUtc >= ExpiresAtUtc)
        {
            throw new RefreshTokenExpiredException();
        }

        RevokedAtUtc = nowUtc;
        ReplacedByTokenId = successorId;
    }

    public override bool Equals(object? obj) =>
        obj is RefreshToken other && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}
