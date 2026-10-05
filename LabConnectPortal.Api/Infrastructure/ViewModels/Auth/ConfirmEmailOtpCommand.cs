namespace LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

public class ConfirmEmailOtpCommand
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
