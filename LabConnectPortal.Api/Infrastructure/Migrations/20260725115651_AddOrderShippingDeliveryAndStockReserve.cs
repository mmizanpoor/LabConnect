using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderShippingDeliveryAndStockReserve : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                schema: "dbo",
                table: "ProductOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedAt",
                schema: "dbo",
                table: "ProductOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                schema: "dbo",
                table: "ProductOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingCompany",
                schema: "dbo",
                table: "ProductOrders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingPhone",
                schema: "dbo",
                table: "ProductOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingRecipientName",
                schema: "dbo",
                table: "ProductOrders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "StockReserved",
                schema: "dbo",
                table: "ProductOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TrackingCode",
                schema: "dbo",
                table: "ProductOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                schema: "dbo",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "ShippedAt",
                schema: "dbo",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                schema: "dbo",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "ShippingCompany",
                schema: "dbo",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "ShippingPhone",
                schema: "dbo",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "ShippingRecipientName",
                schema: "dbo",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "StockReserved",
                schema: "dbo",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "TrackingCode",
                schema: "dbo",
                table: "ProductOrders");
        }
    }
}
