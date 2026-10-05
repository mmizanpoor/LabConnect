using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCenterProfileLabCodeNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LabCodeNew",
                schema: "dbo",
                table: "CenterProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CenterProfiles_LabCodeNew",
                schema: "dbo",
                table: "CenterProfiles",
                column: "LabCodeNew",
                unique: true,
                filter: "[LabCodeNew] IS NOT NULL AND [CenterType] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CenterProfiles_LabCodeNew",
                schema: "dbo",
                table: "CenterProfiles");

            migrationBuilder.DropColumn(
                name: "LabCodeNew",
                schema: "dbo",
                table: "CenterProfiles");
        }
    }
}
