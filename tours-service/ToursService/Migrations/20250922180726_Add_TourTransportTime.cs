using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ToursService.Migrations
{
    /// <inheritdoc />
    public partial class Add_TourTransportTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tour_transport_times",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TourId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Minutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_transport_times", x => x.Id);
                    table.CheckConstraint("CK_TourTransportTime_Minutes_Positive", "\"Minutes\" > 0");
                    table.ForeignKey(
                        name: "FK_tour_transport_times_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tour_transport_times_TourId_Type",
                table: "tour_transport_times",
                columns: new[] { "TourId", "Type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_transport_times");
        }
    }
}
