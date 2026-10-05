namespace LabConnectPortal.Api.Infrastructure.ViewModels.OrganizationLocation;

public class CreateOrganizationLocationCommand
{
    public string LocationName { get; set; } = string.Empty;
    public int ProvinceId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
