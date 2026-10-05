using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingMethodAndCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingCost",
                schema: "dbo",
                table: "ProductOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ShippingMethod",
                schema: "dbo",
                table: "ProductOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultShippingCost",
                schema: "dbo",
                table: "CenterProfiles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingCost",
                schema: "dbo",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "ShippingMethod",
                schema: "dbo",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "DefaultShippingCost",
                schema: "dbo",
                table: "CenterProfiles");
        }
    }
}
