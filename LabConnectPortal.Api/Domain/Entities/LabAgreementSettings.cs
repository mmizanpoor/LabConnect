using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class LabAgreementSettings
{
    [Key]
    public Guid Id { get; set; }

    public Guid CenterProfileId { get; set; }

    /// <summary>When true, only HeaderImage is used; other header fields are ignored.</summary>
    public bool UseHeaderImage { get; set; }

    [MaxLength(500)]
    public string? HeaderImagePath { get; set; }

    [MaxLength(200)]
    public string LabName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string HeaderAddress { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description1 { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? HeaderLogoPath { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public virtual CenterProfile CenterProfile { get; set; } = null!;
}
