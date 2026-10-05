namespace LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

public class RegisterLaboratoryCommand
{
    public int LabCode { get; set; }

    public int LabCodeNew { get; set; }

    public string LabName { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}
