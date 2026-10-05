namespace LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

public class ResetPasswordCommand
{
    public string MobileNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
