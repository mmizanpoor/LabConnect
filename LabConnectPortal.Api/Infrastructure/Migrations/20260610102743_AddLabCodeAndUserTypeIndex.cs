using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLabCodeAndUserTypeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_MobileNumber",
                schema: "dbo",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "LabCode",
                schema: "dbo",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_LabCode",
                schema: "dbo",
                table: "Users",
                column: "LabCode");

            migrationBuilder.CreateIndex(
                name: "IX_Users_MobileNumber_UserType",
                schema: "dbo",
                table: "Users",
                columns: new[] { "MobileNumber", "UserType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_LabCode",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_MobileNumber_UserType",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LabCode",
                schema: "dbo",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_MobileNumber",
                schema: "dbo",
                table: "Users",
                column: "MobileNumber",
                unique: true);
        }
    }
}
