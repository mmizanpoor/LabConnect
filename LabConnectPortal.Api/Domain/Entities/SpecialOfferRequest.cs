using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class SpecialOfferRequest
{
    [Key]
    public long Id { get; set; }

    public long SpecialOfferId { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public SpecialOfferRequestStatus Status { get; set; } = SpecialOfferRequestStatus.Pending;

    [MaxLength(2000)]
    public string? RejectionReason { get; set; }

    public int RequesterLabCodeNew { get; set; }

    public long? LabAgreementId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public SpecialOffer SpecialOffer { get; set; } = null!;

    public User User { get; set; } = null!;

    public LabAgreement? LabAgreement { get; set; }
}
