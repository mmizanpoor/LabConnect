using LabConnectPortal.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    [DbContext(typeof(LabConnectDbContext))]
    [Migration("20260811210000_RemoveContentPostPublishAndPromoFlags")]
    public partial class RemoveContentPostPublishAndPromoFlags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContentPosts_IsPublished",
                schema: "dbo",
                table: "ContentPosts");

            migrationBuilder.DropIndex(
                name: "IX_ContentPosts_UseInPromotions",
                schema: "dbo",
                table: "ContentPosts");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                schema: "dbo",
                table: "ContentPosts");

            migrationBuilder.DropColumn(
                name: "UseInPromotions",
                schema: "dbo",
                table: "ContentPosts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                schema: "dbo",
                table: "ContentPosts",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseInPromotions",
                schema: "dbo",
                table: "ContentPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ContentPosts_IsPublished",
                schema: "dbo",
                table: "ContentPosts",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPosts_UseInPromotions",
                schema: "dbo",
                table: "ContentPosts",
                column: "UseInPromotions");
        }
    }
}
