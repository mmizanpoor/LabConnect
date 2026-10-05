using LabConnectPortal.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(LabConnectDbContext))]
    [Migration("20260811200000_AddContentPostType")]
    public partial class AddContentPostType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "dbo",
                table: "ContentPosts",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("""
                UPDATE p
                SET [Type] = CASE
                    WHEN g.Code IN (N'news', N'News', N'NEWS')
                      OR g.Title LIKE N'%خبر%' THEN 1
                    WHEN g.Code IN (N'articles', N'article', N'Articles')
                      OR g.Title LIKE N'%مقاله%' THEN 2
                    WHEN g.Code IN (N'documents', N'document', N'Documents')
                      OR g.Title LIKE N'%سند%' THEN 3
                    WHEN g.Code IN (N'ads', N'ad', N'Ads', N'promotions')
                      OR g.Title LIKE N'%تبلیغ%' THEN 4
                    ELSE 1
                END
                FROM [dbo].[ContentPosts] p
                INNER JOIN [dbo].[ContentGroups] g ON p.ContentGroupId = g.ContentGroupId;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_ContentPosts_ContentGroups_ContentGroupId",
                schema: "dbo",
                table: "ContentPosts");

            migrationBuilder.AlterColumn<int>(
                name: "ContentGroupId",
                schema: "dbo",
                table: "ContentPosts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentPosts_ContentGroups_ContentGroupId",
                schema: "dbo",
                table: "ContentPosts",
                column: "ContentGroupId",
                principalSchema: "dbo",
                principalTable: "ContentGroups",
                principalColumn: "ContentGroupId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.CreateIndex(
                name: "IX_ContentPosts_Type",
                schema: "dbo",
                table: "ContentPosts",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContentPosts_Type",
                schema: "dbo",
                table: "ContentPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentPosts_ContentGroups_ContentGroupId",
                schema: "dbo",
                table: "ContentPosts");

            migrationBuilder.Sql("""
                UPDATE [dbo].[ContentPosts]
                SET ContentGroupId = (
                    SELECT TOP 1 ContentGroupId FROM [dbo].[ContentGroups] ORDER BY ContentGroupId
                )
                WHERE ContentGroupId IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ContentGroupId",
                schema: "dbo",
                table: "ContentPosts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentPosts_ContentGroups_ContentGroupId",
                schema: "dbo",
                table: "ContentPosts",
                column: "ContentGroupId",
                principalSchema: "dbo",
                principalTable: "ContentGroups",
                principalColumn: "ContentGroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "dbo",
                table: "ContentPosts");
        }
    }
}
