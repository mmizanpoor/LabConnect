using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.UserProfile;

public class ResumeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public BasicInfoDto BasicInfo { get; set; } = new();
    public string AboutMe { get; set; } = string.Empty;
    public PersonalInfoDto PersonalInfo { get; set; } = new();
    public List<UserSkillDto> Skills { get; set; } = [];
    public List<WorkExperienceDto> WorkExperiences { get; set; } = [];
    public List<EducationDto> Educations { get; set; } = [];
    public List<UserLanguageDto> Languages { get; set; } = [];
    public JobPreferenceDto? JobPreference { get; set; }
    public ResumeFileDto? ResumeFile { get; set; }
    public bool IsCompleteForApplication { get; set; }
}

public class BasicInfoDto
{
    public string JobTitle { get; set; } = string.Empty;
    public EmploymentStatus? EmploymentStatus { get; set; }
    public bool HasProfilePhoto { get; set; }
    public string? LatestWorkSummary { get; set; }
    public string? LatestEducationSummary { get; set; }
}

public class PersonalInfoDto
{
    public string Email { get; set; } = string.Empty;
    public string MobilePhone { get; set; } = string.Empty;
    public int? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public string Address { get; set; } = string.Empty;
    public MaritalStatus? MaritalStatus { get; set; }
    public int? BirthYear { get; set; }
    public Gender? Gender { get; set; }
    public MilitaryServiceStatus? MilitaryServiceStatus { get; set; }
}

public class WorkExperienceDto
{
    public Guid? WorkExperienceId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public PersianMonth? StartMonth { get; set; }
    public int? StartYear { get; set; }
    public PersianMonth? EndMonth { get; set; }
    public int? EndYear { get; set; }
    public bool IsCurrentlyEmployed { get; set; } = true;
    public string JobDescription { get; set; } = string.Empty;
}

public class EducationDto
{
    public Guid? EducationId { get; set; }
    public string FieldOfStudy { get; set; } = string.Empty;
    public string InstitutionName { get; set; } = string.Empty;
    public DegreeLevel? DegreeLevel { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public bool IsCurrentlyStudying { get; set; } = true;
    public string Description { get; set; } = string.Empty;
}

public class UserSkillDto
{
    public Guid? UserSkillId { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public SkillProficiencyLevel? ProficiencyLevel { get; set; }
}

public class UserLanguageDto
{
    public Guid? UserLanguageId { get; set; }
    public int LanguageNameId { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public LanguageProficiencyLevel? ProficiencyLevel { get; set; }
}

public class JobPreferenceDto
{
    public List<int> PreferredProvinceIds { get; set; } = [];
    public List<int> JobCategoryIds { get; set; } = [];
    public List<SeniorityLevel> SeniorityLevels { get; set; } = [];
    public List<ContractType> AcceptableContractTypes { get; set; } = [];
    public int? MinimumSalaryId { get; set; }
}

public class ResumeFileDto
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime? UploadedAt { get; set; }
}

public class ReferenceItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SkillSearchResultDto
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
}
