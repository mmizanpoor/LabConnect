using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.LabUser;

public class LabMemberDto
{
    public Guid Id { get; set; }
    public UserType UserType { get; set; }
    public Guid? CenterProfileId { get; set; }
    public int? LabCode { get; set; }
    public int? LabCodeNew { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool MobileConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
}
