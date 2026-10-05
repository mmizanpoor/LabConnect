using LabConnectPortal.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    [DbContext(typeof(LabConnectDbContext))]
    [Migration("20260811220000_AddAdvertisementsAndRemoveContentAds")]
    public partial class AddAdvertisementsAndRemoveContentAds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Advertisements",
                schema: "dbo",
                columns: table => new
                {
                    AdvertisementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advertisements", x => x.AdvertisementId);
                    table.ForeignKey(
                        name: "FK_Advertisements_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_CreatedAt",
                schema: "dbo",
                table: "Advertisements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_CreatedByUserId",
                schema: "dbo",
                table: "Advertisements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_IsActive_StartAt_EndAt",
                schema: "dbo",
                table: "Advertisements",
                columns: new[] { "IsActive", "StartAt", "EndAt" });

            migrationBuilder.Sql("""
                DELETE FROM [dbo].[ContentPosts] WHERE [Type] = 4;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Advertisements",
                schema: "dbo");
        }
    }
}
