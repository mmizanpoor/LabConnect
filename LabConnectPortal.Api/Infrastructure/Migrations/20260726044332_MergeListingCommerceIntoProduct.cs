using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MergeListingCommerceIntoProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                schema: "dbo",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNegotiablePrice",
                schema: "dbo",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                schema: "dbo",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                schema: "dbo",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                schema: "dbo",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // EXEC defers compile until runtime so idempotent scripts can see columns
            // added earlier in the same batch (CreatedByCenterProfileId, Price, ...).
            migrationBuilder.Sql("""
                EXEC(N'
                ;WITH RankedListings AS (
                    SELECT
                        l.ProductId,
                        l.CenterProfileId,
                        l.Price,
                        l.IsNegotiablePrice,
                        l.IsUsed,
                        l.StockQuantity,
                        l.DiscountPercent,
                        ROW_NUMBER() OVER (
                            PARTITION BY l.ProductId, l.CenterProfileId
                            ORDER BY CASE WHEN l.Status = 1 THEN 0 ELSE 1 END, l.UpdatedAt DESC
                        ) AS rn
                    FROM dbo.CenterProductListings AS l
                )
                UPDATE p
                SET
                    p.Price = r.Price,
                    p.IsNegotiablePrice = r.IsNegotiablePrice,
                    p.IsUsed = r.IsUsed,
                    p.StockQuantity = r.StockQuantity,
                    p.DiscountPercent = r.DiscountPercent
                FROM dbo.Products AS p
                INNER JOIN RankedListings AS r
                    ON r.ProductId = p.ProductId
                    AND r.CenterProfileId = p.CreatedByCenterProfileId
                    AND r.rn = 1;

                ;WITH RankedAny AS (
                    SELECT
                        l.ProductId,
                        l.Price,
                        l.IsNegotiablePrice,
                        l.IsUsed,
                        l.StockQuantity,
                        l.DiscountPercent,
                        ROW_NUMBER() OVER (
                            PARTITION BY l.ProductId
                            ORDER BY CASE WHEN l.Status = 1 THEN 0 ELSE 1 END, l.UpdatedAt DESC
                        ) AS rn
                    FROM dbo.CenterProductListings AS l
                )
                UPDATE p
                SET
                    p.Price = r.Price,
                    p.IsNegotiablePrice = r.IsNegotiablePrice,
                    p.IsUsed = r.IsUsed,
                    p.StockQuantity = r.StockQuantity,
                    p.DiscountPercent = r.DiscountPercent
                FROM dbo.Products AS p
                INNER JOIN RankedAny AS r ON r.ProductId = p.ProductId AND r.rn = 1
                WHERE p.Price = 0 AND p.StockQuantity = 1 AND p.IsNegotiablePrice = 1 AND p.IsUsed = 0 AND p.DiscountPercent IS NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM dbo.CenterProductListings x
                      WHERE x.ProductId = p.ProductId AND x.CenterProfileId = p.CreatedByCenterProfileId
                  );
                ');
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_CenterProductListings_CenterProductListingId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_CenterProductListingId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CenterProductListingId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                schema: "dbo",
                table: "CartItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                EXEC(N'
                UPDATE ci
                SET ci.ProductId = l.ProductId
                FROM dbo.CartItems AS ci
                INNER JOIN dbo.CenterProductListings AS l ON l.Id = ci.CenterProductListingId
                WHERE ci.CenterProductListingId IS NOT NULL;
                ');
                """);

            migrationBuilder.DropColumn(
                name: "CenterProductListingId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                schema: "dbo",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId",
                schema: "dbo",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId" },
                unique: true,
                filter: "[ProductId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId",
                schema: "dbo",
                table: "CartItems",
                column: "ProductId",
                principalSchema: "dbo",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_ProductOrderItems_CenterProductListings_CenterProductListingId",
                schema: "dbo",
                table: "ProductOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductOrderItems_CenterProductListingId",
                schema: "dbo",
                table: "ProductOrderItems");

            migrationBuilder.Sql("""
                EXEC(N'
                UPDATE oi
                SET oi.ProductId = COALESCE(oi.ProductId, l.ProductId)
                FROM dbo.ProductOrderItems AS oi
                INNER JOIN dbo.CenterProductListings AS l ON l.Id = oi.CenterProductListingId
                WHERE oi.CenterProductListingId IS NOT NULL;
                ');
                """);

            migrationBuilder.DropColumn(
                name: "CenterProductListingId",
                schema: "dbo",
                table: "ProductOrderItems");

            migrationBuilder.DropTable(
                name: "CenterProductListingImages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CenterProductListings",
                schema: "dbo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ProductId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.CreateTable(
                name: "CenterProductListings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CenterProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsNegotiablePrice = table.Column<bool>(type: "bit", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SellerDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CenterProductListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CenterProductListings_CenterProfiles_CenterProfileId",
                        column: x => x.CenterProfileId,
                        principalSchema: "dbo",
                        principalTable: "CenterProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CenterProductListings_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "dbo",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CenterProductListingImages",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CenterProductListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CenterProductListingImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CenterProductListingImages_CenterProductListings_CenterProductListingId",
                        column: x => x.CenterProductListingId,
                        principalSchema: "dbo",
                        principalTable: "CenterProductListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                EXEC(N'
                INSERT INTO dbo.CenterProductListings
                (Id, CenterProfileId, ProductId, Price, IsNegotiablePrice, IsUsed, StockQuantity, DiscountPercent, Status, PublishedAt, CreatedAt, UpdatedAt)
                SELECT
                    NEWID(),
                    p.CreatedByCenterProfileId,
                    p.ProductId,
                    p.Price,
                    p.IsNegotiablePrice,
                    p.IsUsed,
                    p.StockQuantity,
                    p.DiscountPercent,
                    CASE WHEN p.Status = 2 THEN 1 ELSE 0 END,
                    p.ApprovedAt,
                    p.CreatedAt,
                    p.UpdatedAt
                FROM dbo.Products AS p
                WHERE p.CreatedByCenterProfileId IS NOT NULL;
                ');
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "CenterProductListingId",
                schema: "dbo",
                table: "CartItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                EXEC(N'
                UPDATE ci
                SET ci.CenterProductListingId = l.Id
                FROM dbo.CartItems AS ci
                INNER JOIN dbo.CenterProductListings AS l ON l.ProductId = ci.ProductId
                WHERE ci.ProductId IS NOT NULL;
                ');
                """);

            migrationBuilder.DropColumn(
                name: "ProductId",
                schema: "dbo",
                table: "CartItems");

            migrationBuilder.AddColumn<Guid>(
                name: "CenterProductListingId",
                schema: "dbo",
                table: "ProductOrderItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                EXEC(N'
                UPDATE oi
                SET oi.CenterProductListingId = l.Id
                FROM dbo.ProductOrderItems AS oi
                INNER JOIN dbo.CenterProductListings AS l ON l.ProductId = oi.ProductId
                WHERE oi.ProductId IS NOT NULL AND oi.KitListingId IS NULL;
                ');
                """);

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsNegotiablePrice",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Price",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                schema: "dbo",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrderItems_CenterProductListingId",
                schema: "dbo",
                table: "ProductOrderItems",
                column: "CenterProductListingId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_CenterProductListingId",
                schema: "dbo",
                table: "CartItems",
                columns: new[] { "CartId", "CenterProductListingId" },
                unique: true,
                filter: "[CenterProductListingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CenterProductListingId",
                schema: "dbo",
                table: "CartItems",
                column: "CenterProductListingId");

            migrationBuilder.CreateIndex(
                name: "IX_CenterProductListingImages_CenterProductListingId_SortOrder",
                schema: "dbo",
                table: "CenterProductListingImages",
                columns: new[] { "CenterProductListingId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CenterProductListings_CenterProfileId_ProductId",
                schema: "dbo",
                table: "CenterProductListings",
                columns: new[] { "CenterProfileId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CenterProductListings_ProductId",
                schema: "dbo",
                table: "CenterProductListings",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CenterProductListings_Status_UpdatedAt",
                schema: "dbo",
                table: "CenterProductListings",
                columns: new[] { "Status", "UpdatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_CenterProductListings_CenterProductListingId",
                schema: "dbo",
                table: "CartItems",
                column: "CenterProductListingId",
                principalSchema: "dbo",
                principalTable: "CenterProductListings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductOrderItems_CenterProductListings_CenterProductListingId",
                schema: "dbo",
                table: "ProductOrderItems",
                column: "CenterProductListingId",
                principalSchema: "dbo",
                principalTable: "CenterProductListings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
