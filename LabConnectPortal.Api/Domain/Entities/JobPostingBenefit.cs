using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class JobPostingBenefit
{
    [Key]
    public Guid Id { get; set; }

    public Guid JobPostingId { get; set; }

    [MaxLength(200)]
    public string BenefitText { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public virtual JobPostingRequest JobPosting { get; set; } = null!;
}
