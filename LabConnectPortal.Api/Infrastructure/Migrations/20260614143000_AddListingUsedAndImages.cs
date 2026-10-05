using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddListingUsedAndImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                schema: "dbo",
                table: "CenterProductListings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CenterProductListingImages",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CenterProductListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CenterProductListingImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CenterProductListingImages_CenterProductListings_CenterProductListingId",
                        column: x => x.CenterProductListingId,
                        principalSchema: "dbo",
                        principalTable: "CenterProductListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CenterProductListingImages_CenterProductListingId_SortOrder",
                schema: "dbo",
                table: "CenterProductListingImages",
                columns: new[] { "CenterProductListingId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CenterProductListingImages",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                schema: "dbo",
                table: "CenterProductListings");
        }
    }
}
