using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class alterLabAgreement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryAgreementId",
                schema: "dbo",
                table: "LabAgreements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PrimaryAgreementId",
                schema: "dbo",
                table: "LabAgreements",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
