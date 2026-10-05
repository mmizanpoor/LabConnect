using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductReviewApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE dbo.ProductUserReviews SET Rating = 5 WHERE Rating IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                schema: "dbo",
                table: "ProductUserReviews",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                schema: "dbo",
                table: "ProductUserReviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                schema: "dbo",
                table: "ProductUserReviews",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                schema: "dbo",
                table: "ProductUserReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE dbo.ProductUserReviews SET IsApproved = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                schema: "dbo",
                table: "ProductUserReviews");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                schema: "dbo",
                table: "ProductUserReviews");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                schema: "dbo",
                table: "ProductUserReviews");

            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                schema: "dbo",
                table: "ProductUserReviews",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
