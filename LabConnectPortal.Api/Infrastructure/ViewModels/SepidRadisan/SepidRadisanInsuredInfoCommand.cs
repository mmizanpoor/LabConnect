namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanInsuredInfoCommand : SepidRadisanBaseCommand
{
    public string Insurancer { get; set; } = string.Empty;
    public string InsuredId { get; set; } = string.Empty;
    public string PolicyId { get; set; } = string.Empty;
}
