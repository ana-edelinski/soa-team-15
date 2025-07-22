using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StakeholdersService.Migrations
{
    /// <inheritdoc />
    public partial class AddOptionalFieldsToPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Biography",
                schema: "stakeholders",
                table: "People",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Motto",
                schema: "stakeholders",
                table: "People",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "stakeholders",
                table: "People",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePath",
                schema: "stakeholders",
                table: "People",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                schema: "stakeholders",
                table: "People",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Biography",
                schema: "stakeholders",
                table: "People");

            migrationBuilder.DropColumn(
                name: "Motto",
                schema: "stakeholders",
                table: "People");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "stakeholders",
                table: "People");

            migrationBuilder.DropColumn(
                name: "ProfileImagePath",
                schema: "stakeholders",
                table: "People");

            migrationBuilder.DropColumn(
                name: "Surname",
                schema: "stakeholders",
                table: "People");
        }
    }
}
