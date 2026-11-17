using BookingPlatformInsiders.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatformInsiders.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", table =>
        {
            table.HasCheckConstraint(
                "CK_Payments_Status",
                $"\"Status\" IN ('{PaymentStatus.Initiated}', '{PaymentStatus.Succeeded}', '{PaymentStatus.Failed}', '{PaymentStatus.Expired}')");

            table.HasCheckConstraint("CK_Payments_AttemptNumber", "\"AttemptNumber\" >= 1");
        });

        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Id).ValueGeneratedNever();

        builder.Property(payment => payment.BookingId).IsRequired();

        builder.Property(payment => payment.Provider)
            .HasMaxLength(Payment.MaxProviderLength)
            .IsRequired();

        builder.Property(payment => payment.ProviderPaymentId)
            .HasMaxLength(Payment.MaxProviderPaymentIdLength);

        builder.Property(payment => payment.IdempotencyKey)
            .HasMaxLength(Payment.MaxIdempotencyKeyLength)
            .IsRequired();

        builder.Property(payment => payment.AttemptNumber)
            .IsRequired();

        builder.Property(payment => payment.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(payment => payment.FailureReason)
            .HasMaxLength(Payment.MaxFailureReasonLength);

        builder.Property(payment => payment.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(payment => payment.UpdatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property<uint>("xmin").IsRowVersion();

        builder.HasOne<Domain.Bookings.Booking>()
            .WithMany()
            .HasForeignKey(payment => payment.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(payment => payment.IdempotencyKey).IsUnique();
        builder.HasIndex(payment => new { payment.BookingId, payment.AttemptNumber }).IsUnique();
        builder.HasIndex(payment => new { payment.Provider, payment.ProviderPaymentId })
            .IsUnique()
            .HasFilter("\"ProviderPaymentId\" IS NOT NULL");
        builder.HasIndex(payment => payment.BookingId)
            .IsUnique()
            .HasFilter("\"Status\" = 'Succeeded'")
            .HasDatabaseName("UX_Payments_BookingId_Succeeded");
        builder.HasIndex(payment => payment.BookingId)
            .IsUnique()
            .HasFilter("\"Status\" = 'Initiated'")
            .HasDatabaseName("UX_Payments_BookingId_Initiated");
        builder.HasIndex(payment => new { payment.Status, payment.CreatedAtUtc });
    }
}
