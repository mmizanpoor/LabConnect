using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;

public class CenterProfileListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? LabCode { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public CenterProfileStatus Status { get; set; }
    public bool IsComplete { get; set; }
    public bool IsApproved { get; set; }
    public bool IsApiKeyEnabled { get; set; }
}
