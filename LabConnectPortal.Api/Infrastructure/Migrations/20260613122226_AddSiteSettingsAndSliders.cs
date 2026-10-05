using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteSettingsAndSliders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteSettings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tagline = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SupportLandline = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupportMobile = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ENamadEmbedCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ENamadLinkUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MetaTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MetaKeywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MetaViewport = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MetaCanonical = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SliderGroups",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SliderGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SliderSlides",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SliderGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LinkUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SliderSlides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SliderSlides_SliderGroups_SliderGroupId",
                        column: x => x.SliderGroupId,
                        principalSchema: "dbo",
                        principalTable: "SliderGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "SiteSettings",
                columns: new[] { "Id", "ENamadEmbedCode", "ENamadLinkUrl", "LogoPath", "MetaCanonical", "MetaDescription", "MetaKeywords", "MetaTitle", "MetaViewport", "SiteTitle", "SupportLandline", "SupportMobile", "Tagline", "UpdatedAt" },
                values: new object[] { 1, null, null, null, null, "", "", "", null, "LabConnect Portal", "", "", "", new DateTime(2026, 6, 13, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_SliderGroups_IsActive",
                schema: "dbo",
                table: "SliderGroups",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SliderGroups_StartDate_EndDate",
                schema: "dbo",
                table: "SliderGroups",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SliderSlides_SliderGroupId_SortOrder",
                schema: "dbo",
                table: "SliderSlides",
                columns: new[] { "SliderGroupId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteSettings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SliderSlides",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SliderGroups",
                schema: "dbo");
        }
    }
}
