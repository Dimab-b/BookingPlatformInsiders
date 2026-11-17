using BookingPlatformInsiders.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatformInsiders.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", table => table.HasCheckConstraint(
            "CK_Users_Role",
            $"\"Role\" IN ('{UserRole.User}', '{UserRole.Admin}')"));

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();

        builder.Property(user => user.Email)
            .HasConversion(email => email.Value, value => new Email(value))
            .HasMaxLength(Email.MaxLength)
            .IsRequired();

        builder.HasIndex(user => user.Email).IsUnique();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(User.MaxPasswordHashLength)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(user => user.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();
    }
}
