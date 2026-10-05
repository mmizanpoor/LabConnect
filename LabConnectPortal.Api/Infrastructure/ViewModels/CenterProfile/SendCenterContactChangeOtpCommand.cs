namespace LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;

public class SendCenterContactChangeOtpCommand
{
    /// <summary>Email یا Mobile</summary>
    public string Channel { get; set; } = string.Empty;
}
