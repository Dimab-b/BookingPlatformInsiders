using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingPlatformInsiders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentsAndBookingStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_Status",
                table: "Bookings");

            migrationBuilder.Sql("""
                ALTER TABLE "Bookings"
                DROP CONSTRAINT IF EXISTS "EX_Bookings_NoRoomOverlap";

                ALTER TABLE "Bookings"
                ADD CONSTRAINT "EX_Bookings_NoRoomOverlap"
                EXCLUDE USING gist (
                    "RoomId" WITH =,
                    tstzrange("StartUtc", "EndUtc", '[)') WITH &&
                )
                WHERE ("Status" IN ('Scheduled', 'PendingPayment', 'Completed'));
                """);

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderPaymentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.CheckConstraint("CK_Payments_AttemptNumber", "\"AttemptNumber\" >= 1");
                    table.CheckConstraint("CK_Payments_Status", "\"Status\" IN ('Initiated', 'Succeeded', 'Failed', 'Expired')");
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_Status",
                table: "Bookings",
                sql: "\"Status\" IN ('Scheduled', 'Completed', 'Cancelled', 'PendingPayment', 'PaymentFailed', 'Expired')");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId_AttemptNumber",
                table: "Payments",
                columns: new[] { "BookingId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_IdempotencyKey",
                table: "Payments",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Provider_ProviderPaymentId",
                table: "Payments",
                columns: new[] { "Provider", "ProviderPaymentId" },
                unique: true,
                filter: "\"ProviderPaymentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status_CreatedAtUtc",
                table: "Payments",
                columns: new[] { "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_Payments_BookingId_Initiated",
                table: "Payments",
                column: "BookingId",
                unique: true,
                filter: "\"Status\" = 'Initiated'");

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX "UX_Payments_BookingId_Succeeded"
                ON "Payments" ("BookingId")
                WHERE "Status" = 'Succeeded';

                CREATE INDEX "IX_Payments_BookingId"
                ON "Payments" ("BookingId");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_Status",
                table: "Bookings");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_Status",
                table: "Bookings",
                sql: "\"Status\" IN ('Scheduled', 'Completed', 'Cancelled')");

            migrationBuilder.Sql("""
                ALTER TABLE "Bookings"
                DROP CONSTRAINT IF EXISTS "EX_Bookings_NoRoomOverlap";

                ALTER TABLE "Bookings"
                ADD CONSTRAINT "EX_Bookings_NoRoomOverlap"
                EXCLUDE USING gist (
                    "RoomId" WITH =,
                    tstzrange("StartUtc", "EndUtc", '[)') WITH &&
                )
                WHERE ("Status" <> 'Cancelled');
                """);
        }
    }
}
