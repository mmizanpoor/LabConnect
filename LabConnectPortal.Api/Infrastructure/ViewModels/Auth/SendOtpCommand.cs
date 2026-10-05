using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

public class SendOtpCommand
{
    public string MobileNumber { get; set; } = string.Empty;
    public UserType LoginUserType { get; set; } = UserType.User;
    /// <summary>Optional lab codes for portal SSO login that can auto-create Laboratory/AdminLab users.</summary>
    public int? LabCode { get; set; }
    public int? LabCodeNew { get; set; }
}
