using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class CenterProfile
{
    [Key]
    public Guid Id { get; set; }

    public CenterType CenterType { get; set; }

    public CenterProfileStatus Status { get; set; } = CenterProfileStatus.Pending;

    public int? LabCode { get; set; }

    public int? LabCodeNew { get; set; }

    public Guid? OwnerUserId { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LogoPath { get; set; }

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public int? EstablishedYear { get; set; }

    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Website { get; set; }

    public EmployeeCountRange? EmployeeCount { get; set; }

    [MaxLength(500)]
    public string? NationalCardPath { get; set; }

    [MaxLength(500)]
    public string? LicensePath { get; set; }

    [MaxLength(50)]
    public string? EconomicCode { get; set; }

    [MaxLength(50)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(500)]
    public string? OfficialImagePath { get; set; }

    [MaxLength(500)]
    public string? TradeCardPath { get; set; }

    public bool IsComplete { get; set; }

    public bool IsApproved { get; set; }

    public bool IsApiKeyEnabled { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    public DateTime? RejectedAt { get; set; }

    public Guid? RejectedByUserId { get; set; }
    public int SmsCount { get; set; }
}
