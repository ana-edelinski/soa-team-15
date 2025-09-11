using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToursService.Migrations
{
    /// <inheritdoc />
    public partial class AddPosition2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Position",
                table: "Position");

            migrationBuilder.RenameTable(
                name: "Position",
                newName: "Positions");

            migrationBuilder.RenameIndex(
                name: "IX_Position_TouristId",
                table: "Positions",
                newName: "IX_Positions_TouristId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Positions",
                table: "Positions",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Positions",
                table: "Positions");

            migrationBuilder.RenameTable(
                name: "Positions",
                newName: "Position");

            migrationBuilder.RenameIndex(
                name: "IX_Positions_TouristId",
                table: "Position",
                newName: "IX_Position_TouristId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Position",
                table: "Position",
                column: "Id");
        }
    }
}
