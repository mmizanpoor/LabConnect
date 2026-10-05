using LabConnectPortal.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(LabConnectDbContext))]
    [Migration("20260811140000_AddProductProvince")]
    public partial class AddProductProvince : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProvinceId",
                schema: "dbo",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProvinceId",
                schema: "dbo",
                table: "Products",
                column: "ProvinceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Provinces_ProvinceId",
                schema: "dbo",
                table: "Products",
                column: "ProvinceId",
                principalSchema: "dbo",
                principalTable: "Provinces",
                principalColumn: "ProvinceId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Provinces_ProvinceId",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProvinceId",
                schema: "dbo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                schema: "dbo",
                table: "Products");
        }
    }
}
