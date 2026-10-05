using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyRegulations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyRegulations",
                schema: "dbo",
                columns: table => new
                {
                    CompanyRegulationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyRegulations", x => x.CompanyRegulationId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRegulations_CreatedAt",
                schema: "dbo",
                table: "CompanyRegulations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRegulations_ModifiedAt",
                schema: "dbo",
                table: "CompanyRegulations",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRegulations_Type",
                schema: "dbo",
                table: "CompanyRegulations",
                column: "Type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyRegulations",
                schema: "dbo");
        }
    }
}
