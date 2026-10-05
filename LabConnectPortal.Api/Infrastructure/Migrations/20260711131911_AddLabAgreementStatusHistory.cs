using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLabAgreementStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabAgreementStatusHistories",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabAgreementId = table.Column<long>(type: "bigint", nullable: false),
                    UserType = table.Column<int>(type: "int", nullable: false),
                    PartyActionType = table.Column<int>(type: "int", nullable: false),
                    ActionUserName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ActionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabAgreementStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabAgreementStatusHistories_LabAgreements_LabAgreementId",
                        column: x => x.LabAgreementId,
                        principalSchema: "dbo",
                        principalTable: "LabAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabAgreementStatusHistories_LabAgreementId_ActionDateTime",
                schema: "dbo",
                table: "LabAgreementStatusHistories",
                columns: new[] { "LabAgreementId", "ActionDateTime" });

            migrationBuilder.Sql("""
                INSERT INTO dbo.LabAgreementStatusHistories (LabAgreementId, UserType, PartyActionType, ActionUserName, ActionDateTime)
                SELECT Id, 1, PrimaryAction, ISNULL(PrimaryActionUserName, ''), ISNULL(PrimaryActionDateTime, GETUTCDATE())
                FROM dbo.LabAgreements
                WHERE PrimaryAction IS NOT NULL;

                INSERT INTO dbo.LabAgreementStatusHistories (LabAgreementId, UserType, PartyActionType, ActionUserName, ActionDateTime)
                SELECT Id, 2, ReceiverAction, ISNULL(ReceiverActionUserName, ''), ISNULL(ReceiverActionDateTime, GETUTCDATE())
                FROM dbo.LabAgreements
                WHERE ReceiverAction IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "PrimaryAction",
                schema: "dbo",
                table: "LabAgreements");

            migrationBuilder.DropColumn(
                name: "PrimaryActionDateTime",
                schema: "dbo",
                table: "LabAgreements");

            migrationBuilder.DropColumn(
                name: "PrimaryActionUserName",
                schema: "dbo",
                table: "LabAgreements");

            migrationBuilder.DropColumn(
                name: "ReceiverAction",
                schema: "dbo",
                table: "LabAgreements");

            migrationBuilder.DropColumn(
                name: "ReceiverActionDateTime",
                schema: "dbo",
                table: "LabAgreements");

            migrationBuilder.DropColumn(
                name: "ReceiverActionUserName",
                schema: "dbo",
                table: "LabAgreements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrimaryAction",
                schema: "dbo",
                table: "LabAgreements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrimaryActionDateTime",
                schema: "dbo",
                table: "LabAgreements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryActionUserName",
                schema: "dbo",
                table: "LabAgreements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceiverAction",
                schema: "dbo",
                table: "LabAgreements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceiverActionDateTime",
                schema: "dbo",
                table: "LabAgreements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverActionUserName",
                schema: "dbo",
                table: "LabAgreements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE a
                SET
                    PrimaryAction = h.PartyActionType,
                    PrimaryActionUserName = h.ActionUserName,
                    PrimaryActionDateTime = h.ActionDateTime
                FROM dbo.LabAgreements a
                INNER JOIN (
                    SELECT LabAgreementId, PartyActionType, ActionUserName, ActionDateTime,
                           ROW_NUMBER() OVER (PARTITION BY LabAgreementId ORDER BY ActionDateTime DESC) AS rn
                    FROM dbo.LabAgreementStatusHistories
                    WHERE UserType = 1
                ) h ON h.LabAgreementId = a.Id AND h.rn = 1;

                UPDATE a
                SET
                    ReceiverAction = h.PartyActionType,
                    ReceiverActionUserName = h.ActionUserName,
                    ReceiverActionDateTime = h.ActionDateTime
                FROM dbo.LabAgreements a
                INNER JOIN (
                    SELECT LabAgreementId, PartyActionType, ActionUserName, ActionDateTime,
                           ROW_NUMBER() OVER (PARTITION BY LabAgreementId ORDER BY ActionDateTime DESC) AS rn
                    FROM dbo.LabAgreementStatusHistories
                    WHERE UserType = 2
                ) h ON h.LabAgreementId = a.Id AND h.rn = 1;
                """);

            migrationBuilder.DropTable(
                name: "LabAgreementStatusHistories",
                schema: "dbo");
        }
    }
}
