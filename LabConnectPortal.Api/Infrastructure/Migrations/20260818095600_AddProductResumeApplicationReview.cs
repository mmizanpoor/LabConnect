using LabConnectPortal.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations;

[DbContext(typeof(LabConnectDbContext))]
[Migration("20260818095600_AddProductResumeApplicationReview")]
public partial class AddProductResumeApplicationReview : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'dbo.ProductResumeApplications', N'Status') IS NULL
                ALTER TABLE [dbo].[ProductResumeApplications]
                ADD [Status] int NOT NULL CONSTRAINT [DF_ProductResumeApplications_Status] DEFAULT (0);

            IF COL_LENGTH(N'dbo.ProductResumeApplications', N'ReviewedAt') IS NULL
                ALTER TABLE [dbo].[ProductResumeApplications]
                ADD [ReviewedAt] datetime2 NULL;

            IF COL_LENGTH(N'dbo.ProductResumeApplications', N'ReviewNotes') IS NULL
                ALTER TABLE [dbo].[ProductResumeApplications]
                ADD [ReviewNotes] nvarchar(2000) NOT NULL CONSTRAINT [DF_ProductResumeApplications_ReviewNotes] DEFAULT (N'');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'dbo.ProductResumeApplications', N'ReviewNotes') IS NOT NULL
            BEGIN
                ALTER TABLE [dbo].[ProductResumeApplications] DROP CONSTRAINT [DF_ProductResumeApplications_ReviewNotes];
                ALTER TABLE [dbo].[ProductResumeApplications] DROP COLUMN [ReviewNotes];
            END

            IF COL_LENGTH(N'dbo.ProductResumeApplications', N'ReviewedAt') IS NOT NULL
                ALTER TABLE [dbo].[ProductResumeApplications] DROP COLUMN [ReviewedAt];

            IF COL_LENGTH(N'dbo.ProductResumeApplications', N'Status') IS NOT NULL
            BEGIN
                ALTER TABLE [dbo].[ProductResumeApplications] DROP CONSTRAINT [DF_ProductResumeApplications_Status];
                ALTER TABLE [dbo].[ProductResumeApplications] DROP COLUMN [Status];
            END
            """);
    }
}
