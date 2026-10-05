namespace LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

public class SendPasswordChangeOtpCommand
{
    /// <summary>Email یا Mobile</summary>
    public string Channel { get; set; } = string.Empty;
}
