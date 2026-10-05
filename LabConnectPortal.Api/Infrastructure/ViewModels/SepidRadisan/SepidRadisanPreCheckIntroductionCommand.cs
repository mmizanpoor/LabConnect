namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanPreCheckIntroductionCommand : SepidRadisanBaseCommand
{
    public string Insurancer { get; set; } = string.Empty;
    public string InsuredId { get; set; } = string.Empty;
    public string PolicyId { get; set; } = string.Empty;
    public string DoctorMedicalSystemCode { get; set; } = string.Empty;// کد نظام پزشکی
    public string MedicalSpecialty { get; set; } = string.Empty;// تخصص پزشک
    public List<SepidRadisanServiceInsureDto> ServiceInsures { get; set; } = [];
}
