using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToursService.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tour_transport_times_Tours_TourId",
                table: "tour_transport_times");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tour_transport_times",
                table: "tour_transport_times");

            migrationBuilder.RenameTable(
                name: "tour_transport_times",
                newName: "TourTransportTimes");

            migrationBuilder.RenameIndex(
                name: "IX_tour_transport_times_TourId_Type",
                table: "TourTransportTimes",
                newName: "IX_TourTransportTimes_TourId_Type");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TourTransportTimes",
                table: "TourTransportTimes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TourTransportTimes_Tours_TourId",
                table: "TourTransportTimes",
                column: "TourId",
                principalTable: "Tours",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourTransportTimes_Tours_TourId",
                table: "TourTransportTimes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TourTransportTimes",
                table: "TourTransportTimes");

            migrationBuilder.RenameTable(
                name: "TourTransportTimes",
                newName: "tour_transport_times");

            migrationBuilder.RenameIndex(
                name: "IX_TourTransportTimes_TourId_Type",
                table: "tour_transport_times",
                newName: "IX_tour_transport_times_TourId_Type");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tour_transport_times",
                table: "tour_transport_times",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tour_transport_times_Tours_TourId",
                table: "tour_transport_times",
                column: "TourId",
                principalTable: "Tours",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
