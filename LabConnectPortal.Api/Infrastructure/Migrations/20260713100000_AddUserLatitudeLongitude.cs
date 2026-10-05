using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using LabConnectPortal.Api.Infrastructure.Context;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    [DbContext(typeof(LabConnectDbContext))]
    [Migration("20260713100000_AddUserLatitudeLongitude")]
    public partial class AddUserLatitudeLongitude : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                schema: "dbo",
                table: "Users",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                schema: "dbo",
                table: "Users",
                type: "decimal(9,6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "dbo",
                table: "Users");
        }
    }
}
