using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLabAgreement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabAgreements",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceiverAgreementLabCodeNew = table.Column<int>(type: "int", nullable: false),
                    PrimaryAgreementLabCodeNew = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LaboratoryAgreementState = table.Column<int>(type: "int", nullable: false),
                    PrimaryAgreementId = table.Column<long>(type: "bigint", nullable: false),
                    PrimaryAgreementSign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryAgreementSignDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceiverAgreementSign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiverAgreementSignDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceiverAgreementUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryReturnCause = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiverReturnCause = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GetSampling = table.Column<bool>(type: "bit", nullable: true),
                    GetRecept = table.Column<bool>(type: "bit", nullable: true),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    PrimaryAction = table.Column<int>(type: "int", nullable: true),
                    ReceiverAction = table.Column<int>(type: "int", nullable: true),
                    PrimaryActionUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiverActionUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryActionDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceiverActionDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabAgreements_LabAgreements_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "dbo",
                        principalTable: "LabAgreements",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LabAgreementAttachments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LabAgreementId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabAgreementAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabAgreementAttachments_LabAgreements_LabAgreementId",
                        column: x => x.LabAgreementId,
                        principalSchema: "dbo",
                        principalTable: "LabAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabAgreementTestPrices",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestId = table.Column<long>(type: "bigint", nullable: false),
                    TestName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Approved = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BaseTariffApproved = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FirstAdditions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SecondAdditions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UrgentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CPNCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LabAgreementId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabAgreementTestPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabAgreementTestPrices_LabAgreements_LabAgreementId",
                        column: x => x.LabAgreementId,
                        principalSchema: "dbo",
                        principalTable: "LabAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabAgreementAttachments_LabAgreementId",
                schema: "dbo",
                table: "LabAgreementAttachments",
                column: "LabAgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_LabAgreements_ParentId",
                schema: "dbo",
                table: "LabAgreements",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_LabAgreementTestPrices_LabAgreementId",
                schema: "dbo",
                table: "LabAgreementTestPrices",
                column: "LabAgreementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabAgreementAttachments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LabAgreementTestPrices",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LabAgreements",
                schema: "dbo");
        }
    }
}
