using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTestInfoFullShortName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShortTestName",
                schema: "dbo",
                table: "TestInfos",
                newName: "ShortName");

            migrationBuilder.RenameColumn(
                name: "FullTestName",
                schema: "dbo",
                table: "TestInfos",
                newName: "FullName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShortName",
                schema: "dbo",
                table: "TestInfos",
                newName: "ShortTestName");

            migrationBuilder.RenameColumn(
                name: "FullName",
                schema: "dbo",
                table: "TestInfos",
                newName: "FullTestName");
        }
    }
}
