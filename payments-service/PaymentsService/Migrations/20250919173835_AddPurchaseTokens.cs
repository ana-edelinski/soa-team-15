using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PaymentsService.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ShoppingCarts_CartId",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShoppingCarts",
                table: "ShoppingCarts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.RenameTable(
                name: "ShoppingCarts",
                newName: "shopping_carts");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "order_items");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_CartId",
                table: "order_items",
                newName: "IX_order_items_CartId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "shopping_carts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "shopping_carts",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "order_items",
                type: "numeric(12,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AddColumn<long>(
                name: "AuthorId",
                table: "order_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_shopping_carts",
                table: "shopping_carts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_order_items",
                table: "order_items",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "tour_purchase_tokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TourId = table.Column<long>(type: "bigint", nullable: false),
                    Token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_purchase_tokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tour_purchase_tokens_UserId_TourId",
                table: "tour_purchase_tokens",
                columns: new[] { "UserId", "TourId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_shopping_carts_CartId",
                table: "order_items",
                column: "CartId",
                principalTable: "shopping_carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_shopping_carts_CartId",
                table: "order_items");

            migrationBuilder.DropTable(
                name: "tour_purchase_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_shopping_carts",
                table: "shopping_carts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_order_items",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "shopping_carts");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "shopping_carts");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "order_items");

            migrationBuilder.RenameTable(
                name: "shopping_carts",
                newName: "ShoppingCarts");

            migrationBuilder.RenameTable(
                name: "order_items",
                newName: "OrderItems");

            migrationBuilder.RenameIndex(
                name: "IX_order_items_CartId",
                table: "OrderItems",
                newName: "IX_OrderItems_CartId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "OrderItems",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShoppingCarts",
                table: "ShoppingCarts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ShoppingCarts_CartId",
                table: "OrderItems",
                column: "CartId",
                principalTable: "ShoppingCarts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
