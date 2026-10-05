using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class JobApplication
{
    [Key]
    public Guid JobApplicationId { get; set; }

    public Guid JobPostingId { get; set; }

    public Guid ApplicantUserId { get; set; }

    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.Pending;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public DateTime? ReviewedAt { get; set; }

    [MaxLength(2000)]
    public string ReviewNotes { get; set; } = string.Empty;

    public DateTime? VisitScheduledAt { get; set; }

    [MaxLength(500)]
    public string? VisitLocation { get; set; }

    public virtual JobPostingRequest JobPosting { get; set; } = null!;
    public virtual User ApplicantUser { get; set; } = null!;
}
