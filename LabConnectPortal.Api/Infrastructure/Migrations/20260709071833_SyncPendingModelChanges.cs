using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.ProductOrderItems', 'ShippedAt') IS NULL
                    ALTER TABLE dbo.ProductOrderItems ADD ShippedAt datetime2 NULL;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.KitListingPurchaseRequests', 'ShippedAt') IS NULL
                    ALTER TABLE dbo.KitListingPurchaseRequests ADD ShippedAt datetime2 NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.ProductOrderItems', 'ShippedAt') IS NOT NULL
                    ALTER TABLE dbo.ProductOrderItems DROP COLUMN ShippedAt;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.KitListingPurchaseRequests', 'ShippedAt') IS NOT NULL
                    ALTER TABLE dbo.KitListingPurchaseRequests DROP COLUMN ShippedAt;
                """);
        }
    }
}
