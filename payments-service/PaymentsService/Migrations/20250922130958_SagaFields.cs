using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentsService.Migrations
{
    /// <inheritdoc />
    public partial class SagaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ExecutionId",
                table: "tour_purchase_tokens",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "tour_purchase_tokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedAt",
                table: "tour_purchase_tokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LockedBy",
                table: "tour_purchase_tokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "tour_purchase_tokens",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionId",
                table: "tour_purchase_tokens");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "tour_purchase_tokens");

            migrationBuilder.DropColumn(
                name: "LockedAt",
                table: "tour_purchase_tokens");

            migrationBuilder.DropColumn(
                name: "LockedBy",
                table: "tour_purchase_tokens");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "tour_purchase_tokens");
        }
    }
}
