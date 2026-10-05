using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

public class VerifyOtpCommand
{
    public string MobileNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public UserType LoginUserType { get; set; } = UserType.User;
}
