namespace LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;

public class UpdateLaboratoryMobileCommand
{
    public Guid ProfileId { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
}
