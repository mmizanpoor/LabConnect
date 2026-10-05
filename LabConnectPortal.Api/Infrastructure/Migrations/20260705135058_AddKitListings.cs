using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKitListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KitListings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CenterProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ProductCategoryId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: true),
                    LotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    OriginalPurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PhysicalCondition = table.Column<int>(type: "int", nullable: false),
                    AdditionalDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KitListings_Brands_BrandId",
                        column: x => x.BrandId,
                        principalSchema: "dbo",
                        principalTable: "Brands",
                        principalColumn: "BrandId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KitListings_CenterProfiles_CenterProfileId",
                        column: x => x.CenterProfileId,
                        principalSchema: "dbo",
                        principalTable: "CenterProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KitListings_ProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalSchema: "dbo",
                        principalTable: "ProductCategories",
                        principalColumn: "ProductCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KitListingImages",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KitListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitListingImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KitListingImages_KitListings_KitListingId",
                        column: x => x.KitListingId,
                        principalSchema: "dbo",
                        principalTable: "KitListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KitListingPurchaseRequests",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KitListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedQuantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitListingPurchaseRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KitListingPurchaseRequests_KitListings_KitListingId",
                        column: x => x.KitListingId,
                        principalSchema: "dbo",
                        principalTable: "KitListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KitListingPurchaseRequests_Users_BuyerUserId",
                        column: x => x.BuyerUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KitListingImages_KitListingId_SortOrder",
                schema: "dbo",
                table: "KitListingImages",
                columns: new[] { "KitListingId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_KitListingPurchaseRequests_BuyerUserId",
                schema: "dbo",
                table: "KitListingPurchaseRequests",
                column: "BuyerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KitListingPurchaseRequests_KitListingId_BuyerUserId_Status",
                schema: "dbo",
                table: "KitListingPurchaseRequests",
                columns: new[] { "KitListingId", "BuyerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KitListings_BrandId",
                schema: "dbo",
                table: "KitListings",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_KitListings_CenterProfileId_Status",
                schema: "dbo",
                table: "KitListings",
                columns: new[] { "CenterProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KitListings_ProductCategoryId",
                schema: "dbo",
                table: "KitListings",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_KitListings_Status_UpdatedAt",
                schema: "dbo",
                table: "KitListings",
                columns: new[] { "Status", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KitListingImages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KitListingPurchaseRequests",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KitListings",
                schema: "dbo");
        }
    }
}
