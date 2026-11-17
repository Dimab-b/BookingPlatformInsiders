using BookingPlatformInsiders.Domain.Bookings;
using BookingPlatformInsiders.Domain.Rooms;
using BookingPlatformInsiders.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatformInsiders.Infrastructure.Persistence.Configurations;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings", table =>
        {
            table.HasCheckConstraint("CK_Bookings_TimeRange", "\"EndUtc\" > \"StartUtc\"");
            table.HasCheckConstraint(
                "CK_Bookings_Status",
                $"\"Status\" IN ('{BookingStatus.Scheduled}', '{BookingStatus.Completed}', '{BookingStatus.Cancelled}', '{BookingStatus.PendingPayment}', '{BookingStatus.PaymentFailed}', '{BookingStatus.Expired}')");
        });

        builder.HasKey(booking => booking.Id);
        builder.Property(booking => booking.Id).ValueGeneratedNever();

        builder.Property(booking => booking.Title)
            .HasConversion(title => title.Value, value => new BookingTitle(value))
            .HasMaxLength(BookingTitle.MaxLength)
            .IsRequired();

        builder.OwnsOne(booking => booking.Price, price =>
        {
            price.Property(value => value.AmountOriginal)
                .HasColumnName("AmountOriginal")
                .HasColumnType("numeric(18,2)");

            price.Property(value => value.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);

            price.Property(value => value.AmountEur)
                .HasColumnName("AmountEur")
                .HasColumnType("numeric(18,2)");

            price.Property(value => value.ExchangeRate)
                .HasColumnName("ExchangeRate")
                .HasColumnType("numeric(18,8)");

            price.Property(value => value.RateProvider)
                .HasColumnName("RateProvider")
                .HasMaxLength(BookingPrice.MaxRateProviderLength);

            price.Property(value => value.RateCapturedAtUtc)
                .HasColumnName("RateCapturedAtUtc")
                .HasColumnType("timestamptz");
        });

        builder.Property(booking => booking.PaymentExpiresAtUtc)
            .HasColumnType("timestamptz");

        builder.Property(booking => booking.ClientRequestId)
            .HasMaxLength(Booking.MaxClientRequestIdLength);

        builder.HasIndex(booking => new { booking.OwnerId, booking.ClientRequestId })
            .IsUnique()
            .HasFilter("\"ClientRequestId\" IS NOT NULL");

        builder.Property(booking => booking.StartUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(booking => booking.EndUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(booking => booking.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property<uint>("xmin").IsRowVersion();

        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(booking => booking.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(booking => booking.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(booking => booking.OwnerId);
        builder.HasIndex(booking => new { booking.RoomId, booking.StartUtc, booking.EndUtc });
        builder.HasIndex(booking => new { booking.Status, booking.EndUtc });
    }
}
