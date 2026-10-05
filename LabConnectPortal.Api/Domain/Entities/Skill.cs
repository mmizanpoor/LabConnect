using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class Skill
{
    [Key]
    public int SkillId { get; set; }

    [MaxLength(200)]
    public string SkillName { get; set; } = string.Empty;
}
