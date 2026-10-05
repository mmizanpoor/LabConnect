using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestInfoNameFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullTestName",
                schema: "dbo",
                table: "TestInfos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectionName",
                schema: "dbo",
                table: "TestInfos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortTestName",
                schema: "dbo",
                table: "TestInfos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullTestName",
                schema: "dbo",
                table: "TestInfos");

            migrationBuilder.DropColumn(
                name: "SectionName",
                schema: "dbo",
                table: "TestInfos");

            migrationBuilder.DropColumn(
                name: "ShortTestName",
                schema: "dbo",
                table: "TestInfos");
        }
    }
}
