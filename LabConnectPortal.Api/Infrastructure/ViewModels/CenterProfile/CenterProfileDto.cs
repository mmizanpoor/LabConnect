using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;

public class CenterProfileDto
{
    public Guid Id { get; set; }
    public CenterType CenterType { get; set; }
    public CenterProfileStatus Status { get; set; }
    public int? LabCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int? EstablishedYear { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Website { get; set; }
    public EmployeeCountRange? EmployeeCount { get; set; }
    public string? EconomicCode { get; set; }
    public string? RegistrationNumber { get; set; }
    public bool HasLogo { get; set; }
    public bool HasNationalCard { get; set; }
    public bool HasLicense { get; set; }
    public bool HasOfficialImage { get; set; }
    public bool HasTradeCard { get; set; }
    public bool IsComplete { get; set; }
    public bool IsApproved { get; set; }
    public bool IsApiKeyEnabled { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? RejectedAt { get; set; }
    public int SmsCount { get; set; }
    public bool WasRejected => !string.IsNullOrWhiteSpace(RejectionReason) && !IsComplete && !IsApproved;
}
