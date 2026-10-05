using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addpaymentordertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentOrders",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditQuantity = table.Column<int>(type: "int", nullable: false),
                    SiteChargeServiceCode = table.Column<int>(type: "int", nullable: false),
                    SystemTransactionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankOrderKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentGatewayType = table.Column<int>(type: "int", nullable: false),
                    PaymentState = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StatusCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CardNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CardHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HasFailed = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentOrders_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_BankOrderKey",
                schema: "dbo",
                table: "PaymentOrders",
                column: "BankOrderKey");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_RefId",
                schema: "dbo",
                table: "PaymentOrders",
                column: "RefId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_SystemTransactionKey",
                schema: "dbo",
                table: "PaymentOrders",
                column: "SystemTransactionKey");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_UserId",
                schema: "dbo",
                table: "PaymentOrders",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentOrders",
                schema: "dbo");
        }
    }
}
