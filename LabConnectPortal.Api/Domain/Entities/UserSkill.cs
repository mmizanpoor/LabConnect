using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class UserSkill
{
    [Key]
    public Guid UserSkillId { get; set; }

    public Guid UserId { get; set; }

    public int SkillId { get; set; }

    public SkillProficiencyLevel? ProficiencyLevel { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}
