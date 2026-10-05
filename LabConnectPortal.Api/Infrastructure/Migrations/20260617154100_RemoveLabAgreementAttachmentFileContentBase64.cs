using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLabAgreementAttachmentFileContentBase64 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContentBase64",
                schema: "dbo",
                table: "LabAgreementAttachments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileContentBase64",
                schema: "dbo",
                table: "LabAgreementAttachments",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
