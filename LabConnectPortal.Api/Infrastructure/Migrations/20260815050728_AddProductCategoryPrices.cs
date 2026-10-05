using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCategoryPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[ProductCategoryPrices]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[ProductCategoryPrices] (
                        [ProductCategoryPriceId] int NOT NULL IDENTITY,
                        [ProductCategoryId] int NOT NULL,
                        [Price] decimal(18,2) NOT NULL,
                        [UpdatedAt] datetime2 NOT NULL,
                        [UpdatedByUserId] uniqueidentifier NULL,
                        CONSTRAINT [PK_ProductCategoryPrices] PRIMARY KEY ([ProductCategoryPriceId]),
                        CONSTRAINT [FK_ProductCategoryPrices_ProductCategories_ProductCategoryId]
                            FOREIGN KEY ([ProductCategoryId]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_ProductCategoryPrices_Users_UpdatedByUserId]
                            FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE SET NULL
                    );

                    CREATE UNIQUE INDEX [IX_ProductCategoryPrices_ProductCategoryId]
                        ON [dbo].[ProductCategoryPrices] ([ProductCategoryId]);

                    CREATE INDEX [IX_ProductCategoryPrices_UpdatedByUserId]
                        ON [dbo].[ProductCategoryPrices] ([UpdatedByUserId]);
                END

                UPDATE [dbo].[AdvertisementPositions]
                SET [IsActive] = 0
                WHERE [Code] = N'job_posting';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[ProductCategoryPrices]', N'U') IS NOT NULL
                    DROP TABLE [dbo].[ProductCategoryPrices];

                UPDATE [dbo].[AdvertisementPositions]
                SET [IsActive] = 1
                WHERE [Code] = N'job_posting';
                """);
        }
    }
}
