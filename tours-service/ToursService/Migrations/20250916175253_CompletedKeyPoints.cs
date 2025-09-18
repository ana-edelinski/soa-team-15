using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ToursService.Domain;

#nullable disable

namespace ToursService.Migrations
{
    /// <inheritdoc />
    public partial class CompletedKeyPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<CompletedKeyPoint>>(
                name: "CompletedKeys",
                table: "TourExecution",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Positions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_TourExecution_LocationId",
                table: "TourExecution",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_TourExecution_Positions_LocationId",
                table: "TourExecution",
                column: "LocationId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourExecution_Positions_LocationId",
                table: "TourExecution");

            migrationBuilder.DropIndex(
                name: "IX_TourExecution_LocationId",
                table: "TourExecution");

            migrationBuilder.DropColumn(
                name: "CompletedKeys",
                table: "TourExecution");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Positions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
