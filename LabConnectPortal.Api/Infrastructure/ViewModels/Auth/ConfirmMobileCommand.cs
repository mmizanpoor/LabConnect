namespace LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

public class ConfirmMobileCommand
{
    public string MobileNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
