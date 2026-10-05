using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceUserLabCodesWithCenterProfileId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CenterProfileId",
                schema: "dbo",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE u
                SET u.CenterProfileId = p.Id
                FROM [dbo].[Users] u
                INNER JOIN [dbo].[CenterProfiles] p
                    ON p.CenterType = 0
                    AND (
                        (u.LabCode IS NOT NULL AND p.LabCode = u.LabCode)
                        OR (u.LabCodeNew IS NOT NULL AND p.LabCodeNew = u.LabCodeNew)
                    )
                WHERE u.UserType IN (1, 3)
                  AND u.CenterProfileId IS NULL;

                UPDATE u
                SET u.CenterProfileId = p.Id
                FROM [dbo].[Users] u
                INNER JOIN [dbo].[CenterProfiles] p
                    ON p.CenterType = 1
                    AND p.OwnerUserId = u.Id
                WHERE u.UserType = 2
                  AND u.CenterProfileId IS NULL;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Users_LabCode",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LabCode",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LabCodeNew",
                schema: "dbo",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CenterProfileId",
                schema: "dbo",
                table: "Users",
                column: "CenterProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_CenterProfiles_CenterProfileId",
                schema: "dbo",
                table: "Users",
                column: "CenterProfileId",
                principalSchema: "dbo",
                principalTable: "CenterProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_CenterProfiles_CenterProfileId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CenterProfileId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "LabCode",
                schema: "dbo",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LabCodeNew",
                schema: "dbo",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE u
                SET u.LabCode = p.LabCode,
                    u.LabCodeNew = p.LabCodeNew
                FROM [dbo].[Users] u
                INNER JOIN [dbo].[CenterProfiles] p ON p.Id = u.CenterProfileId
                WHERE u.CenterProfileId IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "CenterProfileId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LabCode",
                schema: "dbo",
                table: "Users",
                column: "LabCode");
        }
    }
}
