using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKitOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                schema: "dbo",
                table: "ProductOrderItems",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "CenterProductListingId",
                schema: "dbo",
                table: "ProductOrderItems",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "KitListingId",
                schema: "dbo",
                table: "ProductOrderItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrderItems_KitListingId",
                schema: "dbo",
                table: "ProductOrderItems",
                column: "KitListingId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductOrderItems_KitListings_KitListingId",
                schema: "dbo",
                table: "ProductOrderItems",
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
                name: "FK_ProductOrderItems_KitListings_KitListingId",
                schema: "dbo",
                table: "ProductOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductOrderItems_KitListingId",
                schema: "dbo",
                table: "ProductOrderItems");

            migrationBuilder.DropColumn(
                name: "KitListingId",
                schema: "dbo",
                table: "ProductOrderItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                schema: "dbo",
                table: "ProductOrderItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CenterProductListingId",
                schema: "dbo",
                table: "ProductOrderItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
