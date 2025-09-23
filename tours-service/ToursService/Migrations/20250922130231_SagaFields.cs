using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToursService.Migrations
{
    /// <inheritdoc />
    public partial class SagaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PurchaseTokenId",
                table: "TourExecution",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SagaId",
                table: "TourExecution",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchaseTokenId",
                table: "TourExecution");

            migrationBuilder.DropColumn(
                name: "SagaId",
                table: "TourExecution");
        }
    }
}
