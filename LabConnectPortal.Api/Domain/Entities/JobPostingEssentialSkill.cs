using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class JobPostingEssentialSkill
{
    [Key]
    public Guid Id { get; set; }

    public Guid JobPostingId { get; set; }

    public int SkillId { get; set; }

    public virtual JobPostingRequest JobPosting { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}
