using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteFooterAndUsefulLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FooterAboutText",
                schema: "dbo",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterAddress",
                schema: "dbo",
                table: "SiteSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FooterCopyrightText",
                schema: "dbo",
                table: "SiteSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FooterEmail",
                schema: "dbo",
                table: "SiteSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SiteUsefulLinks",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteUsefulLinks", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FooterAboutText", "FooterAddress", "FooterCopyrightText", "FooterEmail" },
                values: new object[] { null, "", "", "" });

            migrationBuilder.CreateIndex(
                name: "IX_SiteUsefulLinks_SortOrder",
                schema: "dbo",
                table: "SiteUsefulLinks",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteUsefulLinks",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "FooterAboutText",
                schema: "dbo",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "FooterAddress",
                schema: "dbo",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "FooterCopyrightText",
                schema: "dbo",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "FooterEmail",
                schema: "dbo",
                table: "SiteSettings");
        }
    }
}
