using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCategoryGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductCategoryGroupId",
                schema: "dbo",
                table: "ProductCategories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductCategoryGroups",
                schema: "dbo",
                columns: table => new
                {
                    ProductCategoryGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategoryGroups", x => x.ProductCategoryGroupId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ProductCategoryGroupId",
                schema: "dbo",
                table: "ProductCategories",
                column: "ProductCategoryGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryGroups_Name",
                schema: "dbo",
                table: "ProductCategoryGroups",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_ProductCategoryGroups_ProductCategoryGroupId",
                schema: "dbo",
                table: "ProductCategories",
                column: "ProductCategoryGroupId",
                principalSchema: "dbo",
                principalTable: "ProductCategoryGroups",
                principalColumn: "ProductCategoryGroupId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_ProductCategoryGroups_ProductCategoryGroupId",
                schema: "dbo",
                table: "ProductCategories");

            migrationBuilder.DropTable(
                name: "ProductCategoryGroups",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_ProductCategoryGroupId",
                schema: "dbo",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "ProductCategoryGroupId",
                schema: "dbo",
                table: "ProductCategories");
        }
    }
}
