using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.PublicJob;

public class GetActivePostingsQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public List<int> ProvinceIds { get; set; } = [];
    public List<int> SalaryRangeIds { get; set; } = [];
    public List<ContractType> ContractTypes { get; set; } = [];
    public List<GenderRequirement> GenderRequirements { get; set; } = [];
    public List<DegreeLevel> MinimumDegreeLevels { get; set; } = [];
}

public class PublicJobProvinceFilterDto
{
    public int ProvinceId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PublicJobSalaryFilterDto
{
    public int SalaryRangeId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PublicJobPostingFiltersDto
{
    public List<PublicJobProvinceFilterDto> Provinces { get; set; } = [];
    public List<PublicJobSalaryFilterDto> SalaryRanges { get; set; } = [];
    public List<ContractType> ContractTypes { get; set; } = [];
    public List<GenderRequirement> GenderRequirements { get; set; } = [];
    public List<DegreeLevel> MinimumDegreeLevels { get; set; } = [];
}

public class PublicJobPostingCardDto
{
    public Guid JobPostingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public List<ContractType> ContractTypes { get; set; } = [];
    public string SalaryRangeName { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public bool HasOrganizationLogo { get; set; }
}

public class PublicJobPostingDetailDto
{
    public Guid JobPostingId { get; set; }
    public string JobCategoryName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string SalaryRangeName { get; set; } = string.Empty;
    public int MinimumWorkExperienceYears { get; set; }
    public string JobDescription { get; set; } = string.Empty;
    public List<ContractType> ContractTypes { get; set; } = [];
    public List<string> RequiredPersonalTraits { get; set; } = [];
    public List<string> EssentialSkillNames { get; set; } = [];
    public List<string> Benefits { get; set; } = [];
    public GenderRequirement GenderRequirement { get; set; }
    public JobMilitaryRequirement MilitaryServiceRequirement { get; set; }
    public DegreeLevel MinimumDegreeLevel { get; set; }
    public string AdditionalNotes { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public JobPostingOrgSummaryDto? OrganizationSummary { get; set; }
}

public class JobPostingOrgSummaryDto
{
    public string Name { get; set; } = string.Empty;
    public int? EstablishedYear { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Website { get; set; }
    public EmployeeCountRange? EmployeeCount { get; set; }
    public bool HasLogo { get; set; }
}
