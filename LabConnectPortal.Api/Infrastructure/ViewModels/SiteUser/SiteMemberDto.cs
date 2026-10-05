using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SiteUser;

public class SiteMemberDto
{
    public Guid Id { get; set; }
    public UserType UserType { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool MobileConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AddSiteMemberCommand
{
    public string MobileNumber { get; set; } = string.Empty;
}

public class ConfirmAddSiteMemberCommand
{
    public string MobileNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class AddSiteMemberResultDto
{
    public bool RequiresOtp { get; set; }
    public string OtpScenario { get; set; } = string.Empty;
}

public class ToggleSiteMemberActiveCommand
{
    public Guid MemberId { get; set; }
    public bool IsActive { get; set; }
}

public class RemoveSiteMemberCommand
{
    public Guid MemberId { get; set; }
}
