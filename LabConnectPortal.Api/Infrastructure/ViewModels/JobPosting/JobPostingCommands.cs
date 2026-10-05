using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.JobPosting;

public class SaveJobPostingCommand
{
    public int JobCategoryId { get; set; }
    public Guid LocationId { get; set; }
    public int SalaryRangeId { get; set; }
    public int MinimumWorkExperienceYears { get; set; }
    public string JobDescription { get; set; } = string.Empty;
    public List<ContractType> ContractTypes { get; set; } = [];
    public List<string> RequiredPersonalTraits { get; set; } = [];
    public List<int> EssentialSkillIds { get; set; } = [];
    public List<string> Benefits { get; set; } = [];
    public GenderRequirement GenderRequirement { get; set; }
    public JobMilitaryRequirement MilitaryServiceRequirement { get; set; }
    public DegreeLevel MinimumDegreeLevel { get; set; }
    public string AdditionalNotes { get; set; } = string.Empty;
}

public class UpdateJobPostingCommand : SaveJobPostingCommand
{
    public Guid JobPostingId { get; set; }
}

public class GetMyJobPostingsQuery
{
    public JobPostingStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class JobPostingIdCommand
{
    public Guid JobPostingId { get; set; }
}
