using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoices",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrimaryLabCodeNew = table.Column<int>(type: "int", nullable: false),
                    TargetLabCodeNew = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SentTestIncomeContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentTestIncomeFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderLaboratoryContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderLaboratoryFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CreateDateTime",
                schema: "dbo",
                table: "Invoices",
                column: "CreateDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PrimaryLabCodeNew_TargetLabCodeNew",
                schema: "dbo",
                table: "Invoices",
                columns: new[] { "PrimaryLabCodeNew", "TargetLabCodeNew" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices",
                schema: "dbo");
        }
    }
}
