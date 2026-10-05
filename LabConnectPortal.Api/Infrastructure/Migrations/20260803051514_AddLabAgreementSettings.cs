using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLabAgreementSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabAgreementSettings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CenterProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UseHeaderImage = table.Column<bool>(type: "bit", nullable: false),
                    HeaderImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LabName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HeaderAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description1 = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    HeaderLogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabAgreementSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabAgreementSettings_CenterProfiles_CenterProfileId",
                        column: x => x.CenterProfileId,
                        principalSchema: "dbo",
                        principalTable: "CenterProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabAgreementSettings_CenterProfileId",
                schema: "dbo",
                table: "LabAgreementSettings",
                column: "CenterProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabAgreementSettings",
                schema: "dbo");
        }
    }
}
