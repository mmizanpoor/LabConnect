using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCenterProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CenterProfiles",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CenterType = table.Column<int>(type: "int", nullable: false),
                    LabCode = table.Column<int>(type: "int", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NationalCardPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LicensePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CenterProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CenterProfiles_LabCode",
                schema: "dbo",
                table: "CenterProfiles",
                column: "LabCode",
                unique: true,
                filter: "[LabCode] IS NOT NULL AND [CenterType] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CenterProfiles_OwnerUserId",
                schema: "dbo",
                table: "CenterProfiles",
                column: "OwnerUserId",
                unique: true,
                filter: "[OwnerUserId] IS NOT NULL AND [CenterType] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CenterProfiles",
                schema: "dbo");
        }
    }
}
