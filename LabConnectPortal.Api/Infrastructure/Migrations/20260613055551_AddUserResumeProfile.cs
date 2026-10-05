using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserResumeProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EducationalBackgrounds",
                schema: "dbo",
                columns: table => new
                {
                    EducationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldOfStudy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DegreeLevel = table.Column<int>(type: "int", nullable: true),
                    StartYear = table.Column<int>(type: "int", nullable: true),
                    EndYear = table.Column<int>(type: "int", nullable: true),
                    IsCurrentlyStudying = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalBackgrounds", x => x.EducationId);
                    table.ForeignKey(
                        name: "FK_EducationalBackgrounds_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobCategories",
                schema: "dbo",
                columns: table => new
                {
                    JobCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCategories", x => x.JobCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "LanguageNames",
                schema: "dbo",
                columns: table => new
                {
                    LanguageNameId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageNames", x => x.LanguageNameId);
                });

            migrationBuilder.CreateTable(
                name: "Provinces",
                schema: "dbo",
                columns: table => new
                {
                    ProvinceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProvinceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.ProvinceId);
                });

            migrationBuilder.CreateTable(
                name: "SalaryRanges",
                schema: "dbo",
                columns: table => new
                {
                    SalaryRangeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaryRangeDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryRanges", x => x.SalaryRangeId);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                schema: "dbo",
                columns: table => new
                {
                    SkillId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.SkillId);
                });

            migrationBuilder.CreateTable(
                name: "WorkExperiences",
                schema: "dbo",
                columns: table => new
                {
                    WorkExperienceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartMonth = table.Column<int>(type: "int", nullable: true),
                    StartYear = table.Column<int>(type: "int", nullable: true),
                    EndMonth = table.Column<int>(type: "int", nullable: true),
                    EndYear = table.Column<int>(type: "int", nullable: true),
                    IsCurrentlyEmployed = table.Column<bool>(type: "bit", nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkExperiences", x => x.WorkExperienceId);
                    table.ForeignKey(
                        name: "FK_WorkExperiences_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLanguages",
                schema: "dbo",
                columns: table => new
                {
                    UserLanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageNameId = table.Column<int>(type: "int", nullable: false),
                    ProficiencyLevel = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLanguages", x => x.UserLanguageId);
                    table.ForeignKey(
                        name: "FK_UserLanguages_LanguageNames_LanguageNameId",
                        column: x => x.LanguageNameId,
                        principalSchema: "dbo",
                        principalTable: "LanguageNames",
                        principalColumn: "LanguageNameId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserLanguages_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmploymentStatus = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MobilePhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ProvinceId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MaritalStatus = table.Column<int>(type: "int", nullable: true),
                    BirthYear = table.Column<int>(type: "int", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    MilitaryServiceStatus = table.Column<int>(type: "int", nullable: true),
                    AboutMe = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ProfilePhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ResumeFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ResumeFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    ResumeFileSize = table.Column<long>(type: "bigint", nullable: true),
                    ResumeUploadedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalSchema: "dbo",
                        principalTable: "Provinces",
                        principalColumn: "ProvinceId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobPreferences",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinimumSalaryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPreferences", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_JobPreferences_SalaryRanges_MinimumSalaryId",
                        column: x => x.MinimumSalaryId,
                        principalSchema: "dbo",
                        principalTable: "SalaryRanges",
                        principalColumn: "SalaryRangeId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_JobPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSkills",
                schema: "dbo",
                columns: table => new
                {
                    UserSkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    ProficiencyLevel = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSkills", x => x.UserSkillId);
                    table.ForeignKey(
                        name: "FK_UserSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalSchema: "dbo",
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSkills_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobPreferenceContractTypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPreferenceContractTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPreferenceContractTypes_JobPreferences_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "JobPreferences",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobPreferenceJobCategories",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPreferenceJobCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPreferenceJobCategories_JobCategories_JobCategoryId",
                        column: x => x.JobCategoryId,
                        principalSchema: "dbo",
                        principalTable: "JobCategories",
                        principalColumn: "JobCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobPreferenceJobCategories_JobPreferences_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "JobPreferences",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobPreferenceProvinces",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvinceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPreferenceProvinces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPreferenceProvinces_JobPreferences_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "JobPreferences",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobPreferenceProvinces_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalSchema: "dbo",
                        principalTable: "Provinces",
                        principalColumn: "ProvinceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobPreferenceSeniorityLevels",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeniorityLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPreferenceSeniorityLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPreferenceSeniorityLevels_JobPreferences_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "JobPreferences",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationalBackgrounds_UserId",
                schema: "dbo",
                table: "EducationalBackgrounds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobCategories_CategoryName",
                schema: "dbo",
                table: "JobCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPreferenceContractTypes_UserId_ContractType",
                schema: "dbo",
                table: "JobPreferenceContractTypes",
                columns: new[] { "UserId", "ContractType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPreferenceJobCategories_JobCategoryId",
                schema: "dbo",
                table: "JobPreferenceJobCategories",
                column: "JobCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPreferenceJobCategories_UserId_JobCategoryId",
                schema: "dbo",
                table: "JobPreferenceJobCategories",
                columns: new[] { "UserId", "JobCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPreferenceProvinces_ProvinceId",
                schema: "dbo",
                table: "JobPreferenceProvinces",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPreferenceProvinces_UserId_ProvinceId",
                schema: "dbo",
                table: "JobPreferenceProvinces",
                columns: new[] { "UserId", "ProvinceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobPreferences_MinimumSalaryId",
                schema: "dbo",
                table: "JobPreferences",
                column: "MinimumSalaryId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPreferenceSeniorityLevels_UserId_SeniorityLevel",
                schema: "dbo",
                table: "JobPreferenceSeniorityLevels",
                columns: new[] { "UserId", "SeniorityLevel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LanguageNames_Name",
                schema: "dbo",
                table: "LanguageNames",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_ProvinceName",
                schema: "dbo",
                table: "Provinces",
                column: "ProvinceName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryRanges_SalaryRangeDescription",
                schema: "dbo",
                table: "SalaryRanges",
                column: "SalaryRangeDescription",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_SkillName",
                schema: "dbo",
                table: "Skills",
                column: "SkillName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLanguages_LanguageNameId",
                schema: "dbo",
                table: "UserLanguages",
                column: "LanguageNameId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLanguages_UserId_LanguageNameId",
                schema: "dbo",
                table: "UserLanguages",
                columns: new[] { "UserId", "LanguageNameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_ProvinceId",
                schema: "dbo",
                table: "UserProfiles",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSkills_SkillId",
                schema: "dbo",
                table: "UserSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSkills_UserId_SkillId",
                schema: "dbo",
                table: "UserSkills",
                columns: new[] { "UserId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkExperiences_UserId",
                schema: "dbo",
                table: "WorkExperiences",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EducationalBackgrounds",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "JobPreferenceContractTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "JobPreferenceJobCategories",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "JobPreferenceProvinces",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "JobPreferenceSeniorityLevels",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserLanguages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserProfiles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserSkills",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "WorkExperiences",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "JobCategories",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "JobPreferences",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LanguageNames",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Provinces",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Skills",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SalaryRanges",
                schema: "dbo");
        }
    }
}
