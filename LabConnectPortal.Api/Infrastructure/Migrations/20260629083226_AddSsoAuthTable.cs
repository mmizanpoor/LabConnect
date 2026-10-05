using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSsoAuthTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SSOAuth",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PartnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PartnerPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpireDateSup = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSOAuth", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SSOAuth_Username",
                schema: "dbo",
                table: "SSOAuth",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SSOAuth",
                schema: "dbo");
        }
    }
}
