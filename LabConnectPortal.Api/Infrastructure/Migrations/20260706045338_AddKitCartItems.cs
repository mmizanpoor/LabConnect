using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKitCartItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_CenterProductListingId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "CenterProductListingId",
                schema: "dbo",
                table: "CartItems",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "KitListingId",
                schema: "dbo",
                table: "CartItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_CenterProductListingId",
                schema: "dbo",
                table: "CartItems",
                columns: new[] { "CartId", "CenterProductListingId" },
                unique: true,
                filter: "[CenterProductListingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_KitListingId",
                schema: "dbo",
                table: "CartItems",
                columns: new[] { "CartId", "KitListingId" },
                unique: true,
                filter: "[KitListingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_KitListingId",
                schema: "dbo",
                table: "CartItems",
                column: "KitListingId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_KitListings_KitListingId",
                schema: "dbo",
                table: "CartItems",
                column: "KitListingId",
                principalSchema: "dbo",
                principalTable: "KitListings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_KitListings_KitListingId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_CenterProductListingId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_KitListingId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_KitListingId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "KitListingId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "CenterProductListingId",
                schema: "dbo",
                table: "CartItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_CenterProductListingId",
                schema: "dbo",
                table: "CartItems",
                columns: new[] { "CartId", "CenterProductListingId" },
                unique: true);
        }
    }
}
