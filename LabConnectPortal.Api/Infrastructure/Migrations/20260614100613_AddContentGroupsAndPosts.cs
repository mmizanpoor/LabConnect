using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentGroupsAndPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentGroups",
                schema: "dbo",
                columns: table => new
                {
                    ContentGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentGroups", x => x.ContentGroupId);
                });

            migrationBuilder.CreateTable(
                name: "ContentPosts",
                schema: "dbo",
                columns: table => new
                {
                    ContentPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentGroupId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FullBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeaturedImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ShowOnHomePage = table.Column<bool>(type: "bit", nullable: false),
                    ShowAuthor = table.Column<bool>(type: "bit", nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BrowserTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomMetaTags = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExternalLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPosts", x => x.ContentPostId);
                    table.ForeignKey(
                        name: "FK_ContentPosts_ContentGroups_ContentGroupId",
                        column: x => x.ContentGroupId,
                        principalSchema: "dbo",
                        principalTable: "ContentGroups",
                        principalColumn: "ContentGroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroups_Code",
                schema: "dbo",
                table: "ContentGroups",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ContentGroups_Title",
                schema: "dbo",
                table: "ContentGroups",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentPosts_ContentGroupId",
                schema: "dbo",
                table: "ContentPosts",
                column: "ContentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPosts_IsActive",
                schema: "dbo",
                table: "ContentPosts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPosts_IsPublished",
                schema: "dbo",
                table: "ContentPosts",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPosts_PublishedAt",
                schema: "dbo",
                table: "ContentPosts",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPosts_ShowOnHomePage",
                schema: "dbo",
                table: "ContentPosts",
                column: "ShowOnHomePage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentPosts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ContentGroups",
                schema: "dbo");
        }
    }
}
