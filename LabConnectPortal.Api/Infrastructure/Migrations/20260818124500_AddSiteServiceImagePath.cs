using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteServiceImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[SiteServices]', N'ImagePath') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[SiteServices]
                    ADD [ImagePath] nvarchar(500) NULL;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[SiteServices]', N'ImagePath') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[SiteServices]
                    DROP COLUMN [ImagePath];
                END
                """);
        }
    }
}
