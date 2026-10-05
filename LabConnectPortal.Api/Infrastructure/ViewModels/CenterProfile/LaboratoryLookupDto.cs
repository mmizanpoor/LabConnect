namespace LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;

public class LaboratoryLookupDto
{
    public string Name { get; set; } = string.Empty;

    public int? LabCode { get; set; }

    public int LabCodeNew { get; set; }

    public bool IsApproved { get; set; }

    public string Status { get; set; } = string.Empty;
}
