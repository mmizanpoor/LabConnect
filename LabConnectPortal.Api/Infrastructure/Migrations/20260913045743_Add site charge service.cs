using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addsitechargeservice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteChargeServices",
                schema: "dbo",
                columns: table => new
                {
                    SiteChargeServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PricingMode = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteChargeServices", x => x.SiteChargeServiceId);
                    table.ForeignKey(
                        name: "FK_SiteChargeServices_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SiteChargeServicePrices",
                schema: "dbo",
                columns: table => new
                {
                    SiteChargeServicePriceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteChargeServiceId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MinQuantity = table.Column<int>(type: "int", nullable: true),
                    MaxQuantity = table.Column<int>(type: "int", nullable: true),
                    PackageQuantity = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteChargeServicePrices", x => x.SiteChargeServicePriceId);
                    table.ForeignKey(
                        name: "FK_SiteChargeServicePrices_SiteChargeServices_SiteChargeServiceId",
                        column: x => x.SiteChargeServiceId,
                        principalSchema: "dbo",
                        principalTable: "SiteChargeServices",
                        principalColumn: "SiteChargeServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "SiteChargeServices",
                columns: new[] { "SiteChargeServiceId", "Code", "Description", "IsActive", "PricingMode", "Title", "UpdatedAt", "UpdatedByUserId" },
                values: new object[] { 1, 1, "سرویس شارژ اعتبار پیامک مرکز", true, 0, "شارژ پیامک", new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "SiteChargeServicePrices",
                columns: new[] { "SiteChargeServicePriceId", "MaxQuantity", "MinQuantity", "PackageQuantity", "Price", "SiteChargeServiceId", "SortOrder", "Title" },
                values: new object[] { 1, null, null, null, 200m, 1, 0, null });

            migrationBuilder.CreateIndex(
                name: "IX_SiteChargeServicePrices_SiteChargeServiceId",
                schema: "dbo",
                table: "SiteChargeServicePrices",
                column: "SiteChargeServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteChargeServices_Code",
                schema: "dbo",
                table: "SiteChargeServices",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteChargeServices_UpdatedByUserId",
                schema: "dbo",
                table: "SiteChargeServices",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteChargeServicePrices",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SiteChargeServices",
                schema: "dbo");
        }
    }
}
