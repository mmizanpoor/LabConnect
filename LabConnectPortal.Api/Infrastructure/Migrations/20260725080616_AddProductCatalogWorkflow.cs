using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCatalogWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                schema: "dbo",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                schema: "dbo",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByCenterProfileId",
                schema: "dbo",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "dbo",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeaturedImagePath",
                schema: "dbo",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                schema: "dbo",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "dbo",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                schema: "dbo",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ApprovedByUserId",
                schema: "dbo",
                table: "Products",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedByCenterProfileId",
                schema: "dbo",
                table: "Products",
                column: "CreatedByCenterProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedByUserId",
                schema: "dbo",
                table: "Products",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Status",
                schema: "dbo",
                table: "Products",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CenterProfiles_CreatedByCenterProfileId",
                schema: "dbo",
                table: "Products",
                column: "CreatedByCenterProfileId",
                principalSchema: "dbo",
                principalTable: "CenterProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_ApprovedByUserId",
                schema: "dbo",
                table: "Products",
                column: "ApprovedByUserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_CreatedByUserId",
                schema: "dbo",
                table: "Products",
                column: "CreatedByUserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_CenterProfiles_CreatedByCenterProfileId",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_ApprovedByUserId",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_CreatedByUserId",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ApprovedByUserId",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CreatedByCenterProfileId",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CreatedByUserId",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Status",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedByCenterProfileId",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FeaturedImagePath",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                schema: "dbo",
                table: "Products");
        }
    }
}
