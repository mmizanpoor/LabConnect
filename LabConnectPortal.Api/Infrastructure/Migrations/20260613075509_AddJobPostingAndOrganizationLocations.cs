using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobPostingAndOrganizationLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "CenterProfiles",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCount",
                schema: "dbo",
                table: "CenterProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstablishedYear",
                schema: "dbo",
                table: "CenterProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                schema: "dbo",
                table: "CenterProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrganizationLocations",
                schema: "dbo",
                columns: table => new
                {
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProvinceId = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationLocations", x => x.LocationId);
                    table.ForeignKey(
                        name: "FK_OrganizationLocations_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalSchema: "dbo",
                        principalTable: "Provinces",
                        principalColumn: "ProvinceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationLocations_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobPostingRequests",
                schema: "dbo",
                columns: table => new
                {
                    JobPostingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobCategoryId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalaryRangeId = table.Column<int>(type: "int", nullable: false),
                    MinimumWorkExperienceYears = table.Column<int>(type: "int", nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GenderRequirement = table.Column<int>(type: "int", nullable: false),
                    MilitaryServiceRequirement = table.Column<int>(type: "int", nullable: false),
                    MinimumDegreeLevel = table.Column<int>(type: "int", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingRequests", x => x.JobPostingId);
                    table.ForeignKey(
                        name: "FK_JobPostingRequests_JobCategories_JobCategoryId",
                        column: x => x.JobCategoryId,
                        principalSchema: "dbo",
                        principalTable: "JobCategories",
                        principalColumn: "JobCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobPostingRequests_OrganizationLocations_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "dbo",
                        principalTable: "OrganizationLocations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobPostingRequests_SalaryRanges_SalaryRangeId",
                        column: x => x.SalaryRangeId,
                        principalSchema: "dbo",
                        principalTable: "SalaryRanges",
                        principalColumn: "SalaryRangeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobPostingRequests_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobPostingBenefits",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobPostingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BenefitText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingBenefits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPostingBenefits_JobPostingRequests_JobPostingId",
                        column: x => x.JobPostingId,
                        principalSchema: "dbo",
                        principalTable: "JobPostingRequests",
                        principalColumn: "JobPostingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobPostingContractTypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobPostingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingContractTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPostingContractTypes_JobPostingRequests_JobPostingId",
                        column: x => x.JobPostingId,
                        principalSchema: "dbo",
                        principalTable: "JobPostingRequests",
                        principalColumn: "JobPostingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobPostingEssentialSkills",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobPostingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingEssentialSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPostingEssentialSkills_JobPostingRequests_JobPostingId",
                        column: x => x.JobPostingId,
                        principalSchema: "dbo",
                        principalTable: "JobPostingRequests",
                        principalColumn: "JobPostingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobPostingEssentialSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalSchema: "dbo",
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobPostingPersonalTraits",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobPostingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TraitText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingPersonalTraits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPostingPersonalTraits_JobPostingRequests_JobPostingId",
                        column: x => x.JobPostingId,
                        principalSchema: "dbo",
                        principalTable: "JobPostingRequests",
                        principalColumn: "JobPostingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingBenefits_JobPostingId",
                schema: "dbo",
                table: "JobPostingBenefits",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingContractTypes_JobPostingId_ContractType",
                schema: "dbo",
                table: "JobPostingContractTypes",
                columns: new[] { "JobPostingId", "ContractType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingEssentialSkills_JobPostingId_SkillId",
                schema: "dbo",
                table: "JobPostingEssentialSkills",
                columns: new[] { "JobPostingId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingEssentialSkills_SkillId",
                schema: "dbo",
                table: "JobPostingEssentialSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingPersonalTraits_JobPostingId",
                schema: "dbo",
                table: "JobPostingPersonalTraits",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingRequests_JobCategoryId",
                schema: "dbo",
                table: "JobPostingRequests",
                column: "JobCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingRequests_LocationId",
                schema: "dbo",
                table: "JobPostingRequests",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingRequests_SalaryRangeId",
                schema: "dbo",
                table: "JobPostingRequests",
                column: "SalaryRangeId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingRequests_UserId_Status",
                schema: "dbo",
                table: "JobPostingRequests",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationLocations_ProvinceId",
                schema: "dbo",
                table: "OrganizationLocations",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationLocations_UserId_LocationName",
                schema: "dbo",
                table: "OrganizationLocations",
                columns: new[] { "UserId", "LocationName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPostingBenefits",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "JobPostingContractTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "JobPostingEssentialSkills",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "JobPostingPersonalTraits",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "JobPostingRequests",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "OrganizationLocations",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "dbo",
                table: "CenterProfiles");

            migrationBuilder.DropColumn(
                name: "EmployeeCount",
                schema: "dbo",
                table: "CenterProfiles");

            migrationBuilder.DropColumn(
                name: "EstablishedYear",
                schema: "dbo",
                table: "CenterProfiles");

            migrationBuilder.DropColumn(
                name: "Website",
                schema: "dbo",
                table: "CenterProfiles");
        }
    }
}
