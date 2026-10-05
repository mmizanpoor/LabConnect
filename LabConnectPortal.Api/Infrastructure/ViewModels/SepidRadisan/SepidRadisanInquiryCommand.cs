namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanInquiryCommand : SepidRadisanBaseCommand
{
    public string NationalCode { get; set; } = string.Empty;
    public string Insurancer { get; set; } = string.Empty;
}
