using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceKitGroupsAndTestInfos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceGroups",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabCodeNew = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KitGroups",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabCodeNew = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestInfos",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabCode = table.Column<int>(type: "int", nullable: false),
                    LabCodeNew = table.Column<int>(type: "int", nullable: false),
                    CPNCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NationalCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MeasurName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SimilarName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    KD = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Volume = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MinVolume = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Maintenance = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Transportation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Needs = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Guidance = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PatientInfo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Denial = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Preparation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ClinicalInfo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Sources = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Caution = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SClinical = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Date = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResultDuration = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaxDurResult = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaintenanceDur = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TestId = table.Column<long>(type: "bigint", nullable: false),
                    Criteria = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Freezer = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DeliveryCondition = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApprovePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    KitGroupId = table.Column<long>(type: "bigint", nullable: true),
                    DeviceGroupId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestInfos_DeviceGroups_DeviceGroupId",
                        column: x => x.DeviceGroupId,
                        principalSchema: "dbo",
                        principalTable: "DeviceGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TestInfos_KitGroups_KitGroupId",
                        column: x => x.KitGroupId,
                        principalSchema: "dbo",
                        principalTable: "KitGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGroups_LabCodeNew",
                schema: "dbo",
                table: "DeviceGroups",
                column: "LabCodeNew");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGroups_LabCodeNew_Title",
                schema: "dbo",
                table: "DeviceGroups",
                columns: new[] { "LabCodeNew", "Title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KitGroups_LabCodeNew",
                schema: "dbo",
                table: "KitGroups",
                column: "LabCodeNew");

            migrationBuilder.CreateIndex(
                name: "IX_KitGroups_LabCodeNew_Title",
                schema: "dbo",
                table: "KitGroups",
                columns: new[] { "LabCodeNew", "Title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestInfos_DeviceGroupId",
                schema: "dbo",
                table: "TestInfos",
                column: "DeviceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TestInfos_KitGroupId",
                schema: "dbo",
                table: "TestInfos",
                column: "KitGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TestInfos_LabCodeNew",
                schema: "dbo",
                table: "TestInfos",
                column: "LabCodeNew");

            migrationBuilder.CreateIndex(
                name: "IX_TestInfos_TestId",
                schema: "dbo",
                table: "TestInfos",
                column: "TestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestInfos",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DeviceGroups",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KitGroups",
                schema: "dbo");
        }
    }
}
