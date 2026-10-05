using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialOfferRequestWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "SpecialOfferRequests",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "LabAgreementId",
                schema: "dbo",
                table: "SpecialOfferRequests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                schema: "dbo",
                table: "SpecialOfferRequests",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequesterLabCodeNew",
                schema: "dbo",
                table: "SpecialOfferRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                schema: "dbo",
                table: "SpecialOfferRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                schema: "dbo",
                table: "SpecialOfferRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "dbo",
                table: "SpecialOfferRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                schema: "dbo",
                table: "LabAgreementAttachments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileContentBase64",
                schema: "dbo",
                table: "LabAgreementAttachments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpecialOfferRequests_LabAgreementId",
                schema: "dbo",
                table: "SpecialOfferRequests",
                column: "LabAgreementId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialOfferRequests_LabAgreements_LabAgreementId",
                schema: "dbo",
                table: "SpecialOfferRequests",
                column: "LabAgreementId",
                principalSchema: "dbo",
                principalTable: "LabAgreements",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecialOfferRequests_LabAgreements_LabAgreementId",
                schema: "dbo",
                table: "SpecialOfferRequests");

            migrationBuilder.DropIndex(
                name: "IX_SpecialOfferRequests_LabAgreementId",
                schema: "dbo",
                table: "SpecialOfferRequests");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "dbo",
                table: "SpecialOfferRequests");

            migrationBuilder.DropColumn(
                name: "LabAgreementId",
                schema: "dbo",
                table: "SpecialOfferRequests");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                schema: "dbo",
                table: "SpecialOfferRequests");

            migrationBuilder.DropColumn(
                name: "RequesterLabCodeNew",
                schema: "dbo",
                table: "SpecialOfferRequests");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                schema: "dbo",
                table: "SpecialOfferRequests");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                schema: "dbo",
                table: "SpecialOfferRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "dbo",
                table: "SpecialOfferRequests");

            migrationBuilder.DropColumn(
                name: "ContentType",
                schema: "dbo",
                table: "LabAgreementAttachments");

            migrationBuilder.DropColumn(
                name: "FileContentBase64",
                schema: "dbo",
                table: "LabAgreementAttachments");
        }
    }
}
