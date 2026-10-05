using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryGroupHomePageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomePageImagePath",
                schema: "dbo",
                table: "ProductCategoryGroups",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowOnHomePage",
                schema: "dbo",
                table: "ProductCategoryGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomePageImagePath",
                schema: "dbo",
                table: "ProductCategoryGroups");

            migrationBuilder.DropColumn(
                name: "ShowOnHomePage",
                schema: "dbo",
                table: "ProductCategoryGroups");
        }
    }
}
