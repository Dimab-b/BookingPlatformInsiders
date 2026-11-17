using BookingPlatformInsiders.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatformInsiders.Infrastructure.Persistence.Configurations;

internal sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms", table => table.HasCheckConstraint(
            "CK_Rooms_Capacity",
            $"\"Capacity\" >= {Room.MinCapacity} AND \"Capacity\" <= {Room.MaxCapacity}"));

        builder.HasKey(room => room.Id);
        builder.Property(room => room.Id).ValueGeneratedNever();

        builder.Property(room => room.Name)
            .HasConversion(name => name.Value, value => new RoomName(value))
            .HasMaxLength(RoomName.MaxLength)
            .IsRequired();

        builder.Property(room => room.Capacity)
            .IsRequired();

        builder.Property(room => room.Location)
            .HasMaxLength(Room.MaxLocationLength)
            .IsRequired();

        builder.Property(room => room.Description)
            .HasMaxLength(Room.MaxDescriptionLength);

        builder.Property(room => room.IsPaid)
            .IsRequired();
    }
}
