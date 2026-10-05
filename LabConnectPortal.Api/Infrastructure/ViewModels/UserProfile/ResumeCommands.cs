using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.UserProfile;

public class UpdateBasicInfoCommand
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public EmploymentStatus? EmploymentStatus { get; set; }
}

public class UpdateAboutMeCommand
{
    public string AboutMe { get; set; } = string.Empty;
}

public class UpdatePersonalInfoCommand
{
    public string Email { get; set; } = string.Empty;
    public string MobilePhone { get; set; } = string.Empty;
    public int? ProvinceId { get; set; }
    public string Address { get; set; } = string.Empty;
    public MaritalStatus? MaritalStatus { get; set; }
    public int? BirthYear { get; set; }
    public Gender? Gender { get; set; }
    public MilitaryServiceStatus? MilitaryServiceStatus { get; set; }
}

public class UpdateJobPreferenceCommand
{
    public List<int> PreferredProvinceIds { get; set; } = [];
    public List<int> JobCategoryIds { get; set; } = [];
    public List<SeniorityLevel> SeniorityLevels { get; set; } = [];
    public List<ContractType> AcceptableContractTypes { get; set; } = [];
    public int? MinimumSalaryId { get; set; }
}

public class SaveWorkExperiencesCommand
{
    public List<WorkExperienceDto> Items { get; set; } = [];
}

public class SaveEducationsCommand
{
    public List<EducationDto> Items { get; set; } = [];
}

public class SaveSkillsCommand
{
    public List<UserSkillDto> Items { get; set; } = [];
}

public class SaveLanguagesCommand
{
    public List<UserLanguageDto> Items { get; set; } = [];
}

public class CreateSkillCommand
{
    public string SkillName { get; set; } = string.Empty;
}
