using System;
using LabConnectPortal.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(LabConnectDbContext))]
    [Migration("20260706140000_AddProductOrderItemShippedAt")]
    public partial class AddProductOrderItemShippedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.ProductOrderItems', 'ShippedAt') IS NULL
                    ALTER TABLE dbo.ProductOrderItems ADD ShippedAt datetime2 NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippedAt",
                schema: "dbo",
                table: "ProductOrderItems");
        }
    }
}
