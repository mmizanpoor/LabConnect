using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncAdvertisementCommerceModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AdvertisementOrderId",
                schema: "dbo",
                table: "Advertisements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdvertisementPositionId",
                schema: "dbo",
                table: "Advertisements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkUrl",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "dbo",
                table: "Advertisements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AdvertisementDurations",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DaysCount = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertisementDurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdvertisementPositions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MaxConcurrentSlots = table.Column<int>(type: "int", nullable: false),
                    MaxDisplayCount = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertisementPositions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdvertisementPrices",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdvertisementPositionId = table.Column<int>(type: "int", nullable: false),
                    AdvertisementDurationId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertisementPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvertisementPrices_AdvertisementDurations_AdvertisementDurationId",
                        column: x => x.AdvertisementDurationId,
                        principalSchema: "dbo",
                        principalTable: "AdvertisementDurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvertisementPrices_AdvertisementPositions_AdvertisementPositionId",
                        column: x => x.AdvertisementPositionId,
                        principalSchema: "dbo",
                        principalTable: "AdvertisementPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvertisementPrices_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdvertisementOrders",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdvertisementPositionId = table.Column<int>(type: "int", nullable: false),
                    AdvertisementDurationId = table.Column<int>(type: "int", nullable: false),
                    AdvertisementPriceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertisementOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvertisementOrders_AdvertisementDurations_AdvertisementDurationId",
                        column: x => x.AdvertisementDurationId,
                        principalSchema: "dbo",
                        principalTable: "AdvertisementDurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvertisementOrders_AdvertisementPositions_AdvertisementPositionId",
                        column: x => x.AdvertisementPositionId,
                        principalSchema: "dbo",
                        principalTable: "AdvertisementPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvertisementOrders_AdvertisementPrices_AdvertisementPriceId",
                        column: x => x.AdvertisementPriceId,
                        principalSchema: "dbo",
                        principalTable: "AdvertisementPrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvertisementOrders_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_AdvertisementOrderId",
                schema: "dbo",
                table: "Advertisements",
                column: "AdvertisementOrderId",
                unique: true,
                filter: "[AdvertisementOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_AdvertisementPositionId",
                schema: "dbo",
                table: "Advertisements",
                column: "AdvertisementPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementDurations_Code",
                schema: "dbo",
                table: "AdvertisementDurations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementDurations_SortOrder",
                schema: "dbo",
                table: "AdvertisementDurations",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementOrders_AdvertisementDurationId",
                schema: "dbo",
                table: "AdvertisementOrders",
                column: "AdvertisementDurationId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementOrders_AdvertisementPositionId_Status_StartDate_EndDate",
                schema: "dbo",
                table: "AdvertisementOrders",
                columns: new[] { "AdvertisementPositionId", "Status", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementOrders_AdvertisementPriceId",
                schema: "dbo",
                table: "AdvertisementOrders",
                column: "AdvertisementPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementOrders_UserId",
                schema: "dbo",
                table: "AdvertisementOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementPositions_Code",
                schema: "dbo",
                table: "AdvertisementPositions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementPositions_IsActive",
                schema: "dbo",
                table: "AdvertisementPositions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementPrices_AdvertisementDurationId",
                schema: "dbo",
                table: "AdvertisementPrices",
                column: "AdvertisementDurationId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementPrices_AdvertisementPositionId_AdvertisementDurationId_ValidFrom",
                schema: "dbo",
                table: "AdvertisementPrices",
                columns: new[] { "AdvertisementPositionId", "AdvertisementDurationId", "ValidFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementPrices_CreatedByUserId",
                schema: "dbo",
                table: "AdvertisementPrices",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_AdvertisementOrders_AdvertisementOrderId",
                schema: "dbo",
                table: "Advertisements",
                column: "AdvertisementOrderId",
                principalSchema: "dbo",
                principalTable: "AdvertisementOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_AdvertisementPositions_AdvertisementPositionId",
                schema: "dbo",
                table: "Advertisements",
                column: "AdvertisementPositionId",
                principalSchema: "dbo",
                principalTable: "AdvertisementPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            var seedNow = DateTime.UtcNow.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");

            migrationBuilder.Sql($"""
                SET IDENTITY_INSERT [dbo].[AdvertisementDurations] ON;
                INSERT INTO [dbo].[AdvertisementDurations] ([Id], [Code], [Title], [DaysCount], [SortOrder]) VALUES
                (1, N'daily', N'روزانه', 1, 1),
                (2, N'monthly', N'ماهیانه', 30, 2),
                (3, N'quarterly', N'سه‌ماهه', 90, 3),
                (4, N'semiannual', N'شش‌ماهه', 180, 4),
                (5, N'annual', N'سالیانه', 365, 5);
                SET IDENTITY_INSERT [dbo].[AdvertisementDurations] OFF;

                SET IDENTITY_INSERT [dbo].[AdvertisementPositions] ON;
                INSERT INTO [dbo].[AdvertisementPositions]
                    ([Id], [Code], [Title], [Description], [MaxConcurrentSlots], [MaxDisplayCount], [IsActive], [CreatedAt]) VALUES
                (1, N'job_posting', N'ثبت آگهی', N'هزینه ثبت و انتشار آگهی', 999, NULL, 1, '{seedNow}'),
                (2, N'home_featured', N'تبلیغ ویژه صفحه اول', N'کارت تبلیغ در بخش ویژه صفحه اصلی', 10, 4, 1, '{seedNow}'),
                (3, N'home_banner', N'بنر صفحه اول', N'اسلایدر بنر صفحه اصلی', 5, 5, 1, '{seedNow}'),
                (4, N'search_inline', N'تبلیغ بین جستجوها', N'نمایش بین نتایج جستجوی محصولات', 20, 3, 1, '{seedNow}'),
                (5, N'page_footer', N'تبلیغ پایین صفحات', N'بنر تبلیغاتی در پایین صفحات', 15, 2, 1, '{seedNow}');
                SET IDENTITY_INSERT [dbo].[AdvertisementPositions] OFF;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_AdvertisementOrders_AdvertisementOrderId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_AdvertisementPositions_AdvertisementPositionId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropTable(
                name: "AdvertisementOrders",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AdvertisementPrices",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AdvertisementDurations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AdvertisementPositions",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_AdvertisementOrderId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_AdvertisementPositionId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "AdvertisementOrderId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "AdvertisementPositionId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "LinkUrl",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "dbo",
                table: "Advertisements");
        }
    }
}
