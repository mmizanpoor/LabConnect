namespace LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;

public class RejectCenterProfileCommand
{
    public Guid Id { get; set; }
    public string Reason { get; set; } = string.Empty;
}
