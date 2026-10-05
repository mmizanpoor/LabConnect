using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SmsCount",
                schema: "dbo",
                table: "CenterProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmsCount",
                schema: "dbo",
                table: "CenterProfiles");
        }
    }
}
