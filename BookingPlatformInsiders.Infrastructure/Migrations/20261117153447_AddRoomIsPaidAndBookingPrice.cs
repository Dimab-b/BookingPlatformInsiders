using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingPlatformInsiders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomIsPaidAndBookingPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Rooms",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE "Rooms"
                SET "IsPaid" = true
                WHERE lower("Name") = 'boreal';
                """);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountEur",
                table: "Bookings",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountOriginal",
                table: "Bookings",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientRequestId",
                table: "Bookings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Bookings",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "Bookings",
                type: "numeric(18,8)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentExpiresAtUtc",
                table: "Bookings",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RateCapturedAtUtc",
                table: "Bookings",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RateProvider",
                table: "Bookings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_OwnerId_ClientRequestId",
                table: "Bookings",
                columns: new[] { "OwnerId", "ClientRequestId" },
                unique: true,
                filter: "\"ClientRequestId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_OwnerId_ClientRequestId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "AmountEur",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "AmountOriginal",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ClientRequestId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaymentExpiresAtUtc",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RateCapturedAtUtc",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RateProvider",
                table: "Bookings");
        }
    }
}
