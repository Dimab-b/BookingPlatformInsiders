using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingPlatformInsiders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomNameUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"UX_Rooms_Name\" ON \"Rooms\" (lower(\"Name\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX \"UX_Rooms_Name\";");
        }
    }
}
