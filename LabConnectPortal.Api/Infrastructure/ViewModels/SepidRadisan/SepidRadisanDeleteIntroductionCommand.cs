namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanDeleteIntroductionCommand : SepidRadisanBaseCommand
{
    public string Insurancer { get; set; } = string.Empty;
    public string IntroductionId { get; set; } = string.Empty;
}
