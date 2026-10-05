using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentPostUseInPromotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseInPromotions",
                schema: "dbo",
                table: "ContentPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ContentPosts_UseInPromotions",
                schema: "dbo",
                table: "ContentPosts",
                column: "UseInPromotions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContentPosts_UseInPromotions",
                schema: "dbo",
                table: "ContentPosts");

            migrationBuilder.DropColumn(
                name: "UseInPromotions",
                schema: "dbo",
                table: "ContentPosts");
        }
    }
}
