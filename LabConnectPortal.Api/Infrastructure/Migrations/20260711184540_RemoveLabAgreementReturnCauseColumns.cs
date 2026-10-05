using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLabAgreementReturnCauseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO dbo.LabAgreementStatusHistories (LabAgreementId, UserType, PartyActionType, ActionUserName, ActionDateTime, Reason)
                SELECT
                    a.Id,
                    1,
                    COALESCE(
                        (SELECT TOP 1 h.PartyActionType
                         FROM dbo.LabAgreementStatusHistories h
                         WHERE h.LabAgreementId = a.Id AND h.UserType = 1
                         ORDER BY h.ActionDateTime DESC),
                        1),
                    '',
                    GETUTCDATE(),
                    a.PrimaryReturnCause
                FROM dbo.LabAgreements a
                WHERE a.PrimaryReturnCause IS NOT NULL
                  AND LTRIM(RTRIM(a.PrimaryReturnCause)) <> ''
                  AND NOT EXISTS (
                      SELECT 1
                      FROM dbo.LabAgreementStatusHistories h
                      WHERE h.LabAgreementId = a.Id
                        AND h.UserType = 1
                        AND h.Reason IS NOT NULL
                        AND LTRIM(RTRIM(h.Reason)) <> '');

                INSERT INTO dbo.LabAgreementStatusHistories (LabAgreementId, UserType, PartyActionType, ActionUserName, ActionDateTime, Reason)
                SELECT
                    a.Id,
                    2,
                    COALESCE(
                        (SELECT TOP 1 h.PartyActionType
                         FROM dbo.LabAgreementStatusHistories h
                         WHERE h.LabAgreementId = a.Id AND h.UserType = 2
                         ORDER BY h.ActionDateTime DESC),
                        1),
                    '',
                    GETUTCDATE(),
                    a.ReceiverReturnCause
                FROM dbo.LabAgreements a
                WHERE a.ReceiverReturnCause IS NOT NULL
                  AND LTRIM(RTRIM(a.ReceiverReturnCause)) <> ''
                  AND NOT EXISTS (
                      SELECT 1
                      FROM dbo.LabAgreementStatusHistories h
                      WHERE h.LabAgreementId = a.Id
                        AND h.UserType = 2
                        AND h.Reason IS NOT NULL
                        AND LTRIM(RTRIM(h.Reason)) <> '');
                """);

            migrationBuilder.DropColumn(
                name: "PrimaryReturnCause",
                schema: "dbo",
                table: "LabAgreements");

            migrationBuilder.DropColumn(
                name: "ReceiverReturnCause",
                schema: "dbo",
                table: "LabAgreements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrimaryReturnCause",
                schema: "dbo",
                table: "LabAgreements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverReturnCause",
                schema: "dbo",
                table: "LabAgreements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE a
                SET PrimaryReturnCause = h.Reason
                FROM dbo.LabAgreements a
                INNER JOIN (
                    SELECT LabAgreementId, Reason,
                           ROW_NUMBER() OVER (PARTITION BY LabAgreementId ORDER BY ActionDateTime DESC) AS rn
                    FROM dbo.LabAgreementStatusHistories
                    WHERE UserType = 1 AND Reason IS NOT NULL
                ) h ON h.LabAgreementId = a.Id AND h.rn = 1;

                UPDATE a
                SET ReceiverReturnCause = h.Reason
                FROM dbo.LabAgreements a
                INNER JOIN (
                    SELECT LabAgreementId, Reason,
                           ROW_NUMBER() OVER (PARTITION BY LabAgreementId ORDER BY ActionDateTime DESC) AS rn
                    FROM dbo.LabAgreementStatusHistories
                    WHERE UserType = 2 AND Reason IS NOT NULL
                ) h ON h.LabAgreementId = a.Id AND h.rn = 1;
                """);
        }
    }
}
