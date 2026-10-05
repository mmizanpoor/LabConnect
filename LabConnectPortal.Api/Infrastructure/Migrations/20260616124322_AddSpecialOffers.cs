using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpecialOffers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabCodeNew = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FullBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialOffers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpecialOfferRequests",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecialOfferId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialOfferRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialOfferRequests_SpecialOffers_SpecialOfferId",
                        column: x => x.SpecialOfferId,
                        principalSchema: "dbo",
                        principalTable: "SpecialOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpecialOfferRequests_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SpecialOfferTests",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecialOfferId = table.Column<long>(type: "bigint", nullable: false),
                    TestInfoId = table.Column<long>(type: "bigint", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    MaxSamples = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialOfferTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialOfferTests_SpecialOffers_SpecialOfferId",
                        column: x => x.SpecialOfferId,
                        principalSchema: "dbo",
                        principalTable: "SpecialOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpecialOfferTests_TestInfos_TestInfoId",
                        column: x => x.TestInfoId,
                        principalSchema: "dbo",
                        principalTable: "TestInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpecialOfferRequests_SpecialOfferId_UserId",
                schema: "dbo",
                table: "SpecialOfferRequests",
                columns: new[] { "SpecialOfferId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpecialOfferRequests_UserId",
                schema: "dbo",
                table: "SpecialOfferRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialOffers_IsActive_EndDate",
                schema: "dbo",
                table: "SpecialOffers",
                columns: new[] { "IsActive", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SpecialOffers_LabCodeNew",
                schema: "dbo",
                table: "SpecialOffers",
                column: "LabCodeNew");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialOfferTests_SpecialOfferId_TestInfoId",
                schema: "dbo",
                table: "SpecialOfferTests",
                columns: new[] { "SpecialOfferId", "TestInfoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpecialOfferTests_TestInfoId",
                schema: "dbo",
                table: "SpecialOfferTests",
                column: "TestInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecialOfferRequests",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SpecialOfferTests",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SpecialOffers",
                schema: "dbo");
        }
    }
}
