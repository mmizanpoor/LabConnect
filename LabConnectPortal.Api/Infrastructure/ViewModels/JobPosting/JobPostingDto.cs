using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.JobPosting;

public class JobPostingOrgSummaryDto
{
    public string Name { get; set; } = string.Empty;
    public int? EstablishedYear { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Website { get; set; }
    public EmployeeCountRange? EmployeeCount { get; set; }
    public bool HasLogo { get; set; }
}

public class JobPostingDto
{
    public Guid JobPostingId { get; set; }
    public int JobCategoryId { get; set; }
    public string JobCategoryName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int SalaryRangeId { get; set; }
    public string SalaryRangeName { get; set; } = string.Empty;
    public int MinimumWorkExperienceYears { get; set; }
    public string JobDescription { get; set; } = string.Empty;
    public List<ContractType> ContractTypes { get; set; } = [];
    public List<string> RequiredPersonalTraits { get; set; } = [];
    public List<int> EssentialSkillIds { get; set; } = [];
    public List<string> EssentialSkillNames { get; set; } = [];
    public List<string> Benefits { get; set; } = [];
    public GenderRequirement GenderRequirement { get; set; }
    public JobMilitaryRequirement MilitaryServiceRequirement { get; set; }
    public DegreeLevel MinimumDegreeLevel { get; set; }
    public string AdditionalNotes { get; set; } = string.Empty;
    public JobPostingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public JobPostingOrgSummaryDto? OrganizationSummary { get; set; }
}

public class JobPostingListItemDto
{
    public Guid JobPostingId { get; set; }
    public string JobCategoryName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public JobPostingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}
