using LabConnectPortal.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    [DbContext(typeof(LabConnectDbContext))]
    [Migration("20260923120000_AddInvoiceState")]
    public partial class AddInvoiceState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "State",
                schema: "dbo",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                schema: "dbo",
                table: "Invoices");
        }
    }
}
