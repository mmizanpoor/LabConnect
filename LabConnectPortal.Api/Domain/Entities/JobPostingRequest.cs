using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class JobPostingRequest
{
    [Key]
    public Guid JobPostingId { get; set; }

    public Guid UserId { get; set; }

    public int JobCategoryId { get; set; }

    public Guid LocationId { get; set; }

    public int SalaryRangeId { get; set; }

    public int MinimumWorkExperienceYears { get; set; }

    public string JobDescription { get; set; } = string.Empty;

    public GenderRequirement GenderRequirement { get; set; }

    public JobMilitaryRequirement MilitaryServiceRequirement { get; set; }

    public DegreeLevel MinimumDegreeLevel { get; set; }

    public string AdditionalNotes { get; set; } = string.Empty;

    public JobPostingStatus Status { get; set; } = JobPostingStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public DateTime? PublishedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual JobCategory JobCategory { get; set; } = null!;
    public virtual OrganizationLocation Location { get; set; } = null!;
    public virtual SalaryRange SalaryRange { get; set; } = null!;
    public virtual ICollection<JobPostingContractType> ContractTypes { get; set; } = new List<JobPostingContractType>();
    public virtual ICollection<JobPostingEssentialSkill> EssentialSkills { get; set; } = new List<JobPostingEssentialSkill>();
    public virtual ICollection<JobPostingPersonalTrait> PersonalTraits { get; set; } = new List<JobPostingPersonalTrait>();
    public virtual ICollection<JobPostingBenefit> Benefits { get; set; } = new List<JobPostingBenefit>();
    public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
