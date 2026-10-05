namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanAppendAttachmentCommand : SepidRadisanBaseCommand
{
    public string Insurancer { get; set; } = string.Empty;
    public string IntroductionId { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public SepidRadisanUploadFileDto[] Files { get; set; } = [];
}
