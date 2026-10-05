using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKitListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Former SystemEntity.KitListing — remove leftover lab permissions.
            migrationBuilder.Sql("""
                DELETE FROM [dbo].[LabUserPermissions]
                WHERE [SystemEntityId] = 'a6cfa2b0-7cda-4845-b5df-da3d79f81252';
                """);

            // Remove kit-only cart rows before dropping KitListingId.
            migrationBuilder.Sql("""
                DELETE FROM [dbo].[CartItems]
                WHERE [KitListingId] IS NOT NULL;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_KitListings_KitListingId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductOrderItems_KitListings_KitListingId",
                schema: "dbo",
                table: "ProductOrderItems");

            migrationBuilder.DropTable(
                name: "KitListingImages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KitListingPurchaseRequests",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KitListings",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_ProductOrderItems_KitListingId",
                schema: "dbo",
                table: "ProductOrderItems");

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
                table: "ProductOrderItems");

            migrationBuilder.DropColumn(
                name: "KitListingId",
                schema: "dbo",
                table: "CartItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KitListingId",
                schema: "dbo",
                table: "ProductOrderItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KitListingId",
                schema: "dbo",
                table: "CartItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KitListings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: true),
                    CenterProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCategoryId = table.Column<int>(type: "int", nullable: false),
                    AdditionalDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalPurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PhysicalCondition = table.Column<int>(type: "int", nullable: false),
                    ProductionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
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
                    BuyerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KitListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RequestedQuantity = table.Column<int>(type: "int", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ShippedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_ProductOrderItems_KitListingId",
                schema: "dbo",
                table: "ProductOrderItems",
                column: "KitListingId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_KitListings_KitListingId",
                schema: "dbo",
                table: "CartItems",
                column: "KitListingId",
                principalSchema: "dbo",
                principalTable: "KitListings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
    }
}
