using System;
using LabConnectPortal.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(LabConnectDbContext))]
    [Migration("20260704110000_AddSiteVisitsAndUserLastSeenAt")]
    public partial class AddSiteVisitsAndUserLastSeenAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Users', 'LastSeenAt') IS NULL
                    ALTER TABLE dbo.Users ADD LastSeenAt datetime2 NULL;
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID('dbo.SiteVisits', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.SiteVisits (
                        Id uniqueidentifier NOT NULL,
                        IpHash nvarchar(64) NOT NULL,
                        ViewedAt datetime2 NOT NULL,
                        CONSTRAINT PK_SiteVisits PRIMARY KEY (Id)
                    );
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_SiteVisits_ViewedAt' AND object_id = OBJECT_ID('dbo.SiteVisits'))
                    CREATE INDEX IX_SiteVisits_ViewedAt ON dbo.SiteVisits (ViewedAt);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_SiteVisits_IpHash_ViewedAt' AND object_id = OBJECT_ID('dbo.SiteVisits'))
                    CREATE INDEX IX_SiteVisits_IpHash_ViewedAt ON dbo.SiteVisits (IpHash, ViewedAt);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Users_LastSeenAt' AND object_id = OBJECT_ID('dbo.Users'))
                    CREATE INDEX IX_Users_LastSeenAt ON dbo.Users (LastSeenAt);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteVisits",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastSeenAt",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                schema: "dbo",
                table: "Users");
        }
    }
}
