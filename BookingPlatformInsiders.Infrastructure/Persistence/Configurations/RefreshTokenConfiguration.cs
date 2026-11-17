using BookingPlatformInsiders.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatformInsiders.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", table => table.HasCheckConstraint(
            "CK_RefreshTokens_Expiry",
            "\"ExpiresAtUtc\" > \"CreatedAtUtc\""));

        builder.HasKey(token => token.Id);
        builder.Property(token => token.Id).ValueGeneratedNever();

        builder.Property(token => token.TokenHash)
            .HasMaxLength(RefreshToken.MaxTokenHashLength)
            .IsRequired();

        builder.HasIndex(token => token.TokenHash).IsUnique();

        builder.Property(token => token.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(token => token.ExpiresAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(token => token.RevokedAtUtc)
            .HasColumnType("timestamptz");

        builder.Property<uint>("xmin").IsRowVersion();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RefreshToken>()
            .WithMany()
            .HasForeignKey(token => token.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(token => new { token.FamilyId, token.RevokedAtUtc });
        builder.HasIndex(token => new { token.UserId, token.RevokedAtUtc });
        builder.HasIndex(token => token.ExpiresAtUtc);
    }
}
