using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class WorkExperience
{
    [Key]
    public Guid WorkExperienceId { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    public PersianMonth? StartMonth { get; set; }

    public int? StartYear { get; set; }

    public PersianMonth? EndMonth { get; set; }

    public int? EndYear { get; set; }

    public bool IsCurrentlyEmployed { get; set; } = true;

    [MaxLength(4000)]
    public string JobDescription { get; set; } = string.Empty;

    public virtual User User { get; set; } = null!;
}
