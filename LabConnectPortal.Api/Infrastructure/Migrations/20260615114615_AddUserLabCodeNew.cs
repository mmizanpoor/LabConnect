using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLabCodeNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LabCodeNew",
                schema: "dbo",
                table: "Users",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LabCodeNew",
                schema: "dbo",
                table: "Users");
        }
    }
}
