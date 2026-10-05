using LabConnectPortal.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(LabConnectDbContext))]
    [Migration("20260817133000_AddCategoryAcceptsResumeAndProductResumeApplications")]
    public partial class AddCategoryAcceptsResumeAndProductResumeApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ProductCategories', N'AcceptsResume') IS NULL
                    ALTER TABLE [dbo].[ProductCategories]
                    ADD [AcceptsResume] bit NOT NULL CONSTRAINT [DF_ProductCategories_AcceptsResume] DEFAULT (0);

                IF OBJECT_ID(N'[dbo].[ProductResumeApplications]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[ProductResumeApplications] (
                        [ProductResumeApplicationId] uniqueidentifier NOT NULL,
                        [ProductId] uniqueidentifier NOT NULL,
                        [ApplicantUserId] uniqueidentifier NOT NULL,
                        [SubmittedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_ProductResumeApplications] PRIMARY KEY ([ProductResumeApplicationId]),
                        CONSTRAINT [FK_ProductResumeApplications_Products_ProductId]
                            FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_ProductResumeApplications_Users_ApplicantUserId]
                            FOREIGN KEY ([ApplicantUserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION
                    );

                    CREATE UNIQUE INDEX [IX_ProductResumeApplications_ProductId_ApplicantUserId]
                        ON [dbo].[ProductResumeApplications] ([ProductId], [ApplicantUserId]);

                    CREATE INDEX [IX_ProductResumeApplications_ApplicantUserId]
                        ON [dbo].[ProductResumeApplications] ([ApplicantUserId]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[ProductResumeApplications]', N'U') IS NOT NULL
                    DROP TABLE [dbo].[ProductResumeApplications];

                IF COL_LENGTH(N'dbo.ProductCategories', N'AcceptsResume') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[ProductCategories] DROP CONSTRAINT [DF_ProductCategories_AcceptsResume];
                    ALTER TABLE [dbo].[ProductCategories] DROP COLUMN [AcceptsResume];
                END
                """);
        }
    }
}
