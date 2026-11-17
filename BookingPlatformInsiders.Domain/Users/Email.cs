using BookingPlatformInsiders.Domain.Common.Exceptions;

namespace BookingPlatformInsiders.Domain.Users;

public sealed record Email
{
    public const int MaxLength = 256;

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidEmailException();
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength || !IsValid(normalized))
        {
            throw new InvalidEmailException();
        }

        Value = normalized;
    }

    public override string ToString() => Value;

    private static bool IsValid(string value)
    {
        if (value.Contains(' '))
        {
            return false;
        }

        var atIndex = value.IndexOf('@');

        if (atIndex <= 0 || atIndex != value.LastIndexOf('@') || atIndex == value.Length - 1)
        {
            return false;
        }

        var domain = value[(atIndex + 1)..];

        return domain.Contains('.')
            && !domain.StartsWith('.')
            && !domain.EndsWith('.');
    }
}
