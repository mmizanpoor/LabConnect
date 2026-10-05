using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class EducationalBackground
{
    [Key]
    public Guid EducationId { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(200)]
    public string FieldOfStudy { get; set; } = string.Empty;

    [MaxLength(200)]
    public string InstitutionName { get; set; } = string.Empty;

    public DegreeLevel? DegreeLevel { get; set; }

    public int? StartYear { get; set; }

    public int? EndYear { get; set; }

    public bool IsCurrentlyStudying { get; set; } = true;

    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    public virtual User User { get; set; } = null!;
}
