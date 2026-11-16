using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingPlatformInsiders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingOverlapConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql("""
                ALTER TABLE "Bookings"
                ADD CONSTRAINT "EX_Bookings_NoRoomOverlap"
                EXCLUDE USING gist (
                    "RoomId" WITH =,
                    tstzrange("StartUtc", "EndUtc", '[)') WITH &&
                )
                WHERE ("Status" <> 'Cancelled');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Bookings"
                DROP CONSTRAINT "EX_Bookings_NoRoomOverlap";
                """);
        }
    }
}
