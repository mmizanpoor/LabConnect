using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteServicesSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteServices",
                schema: "dbo",
                columns: table => new
                {
                    SiteServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteServices", x => x.SiteServiceId);
                });

            migrationBuilder.Sql("""
                IF NOT EXISTS(SELECT 1 FROM [dbo].[SiteServices])
                BEGIN
                    INSERT INTO [dbo].[SiteServices] ([Title], [Description], [ImagePath], [LinkUrl], [IsActive], [SortOrder])
                    VALUES
                        (N'تجهیزات آزمایشگاهی', NULL, NULL, NULL, 1, 0),
                        (N'خدمات و بازرگانی آزمایشگاه', NULL, NULL, NULL, 1, 1),
                        (N'کیت و مواد مصرفی', NULL, NULL, NULL, 1, 2),
                        (N'کاریابی و استخدام', NULL, NULL, NULL, 1, 3);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteServices",
                schema: "dbo");
        }
    }
}
