using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class ProductResumeApplication
{
    [Key]
    public Guid ProductResumeApplicationId { get; set; }

    public Guid ProductId { get; set; }

    public Guid ApplicantUserId { get; set; }

    public ProductResumeApplicationStatus Status { get; set; } = ProductResumeApplicationStatus.Pending;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public DateTime? ReviewedAt { get; set; }

    [MaxLength(2000)]
    public string ReviewNotes { get; set; } = string.Empty;

    public virtual Product Product { get; set; } = null!;
    public virtual User ApplicantUser { get; set; } = null!;
}
