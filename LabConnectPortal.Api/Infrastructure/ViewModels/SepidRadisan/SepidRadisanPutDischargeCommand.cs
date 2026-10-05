namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanPutDischargeCommand : SepidRadisanBaseCommand
{
    public string Insurancer { get; set; } = string.Empty;
    public string IntroductionId { get; set; } = string.Empty;
    public string DischargeDate { get; set; } = string.Empty;
}
