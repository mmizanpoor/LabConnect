using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCenterProfileCompanyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EconomicCode",
                schema: "dbo",
                table: "CenterProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficialImagePath",
                schema: "dbo",
                table: "CenterProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                schema: "dbo",
                table: "CenterProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradeCardPath",
                schema: "dbo",
                table: "CenterProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EconomicCode",
                schema: "dbo",
                table: "CenterProfiles");

            migrationBuilder.DropColumn(
                name: "OfficialImagePath",
                schema: "dbo",
                table: "CenterProfiles");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                schema: "dbo",
                table: "CenterProfiles");

            migrationBuilder.DropColumn(
                name: "TradeCardPath",
                schema: "dbo",
                table: "CenterProfiles");
        }
    }
}
