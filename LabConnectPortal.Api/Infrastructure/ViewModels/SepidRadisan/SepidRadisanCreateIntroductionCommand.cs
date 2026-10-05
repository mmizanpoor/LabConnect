namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanCreateIntroductionCommand : SepidRadisanBaseCommand
{
    public string Insurancer { get; set; } = string.Empty;
    public string InsuredId { get; set; } = string.Empty;
    public string PolicyId { get; set; } = string.Empty;
    public string Checksum { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorCode { get; set; } = string.Empty;
    public string MedicalSpecialty { get; set; } = string.Empty;
    public string DoctorOrderDate { get; set; } = string.Empty;
    public string ReceptionDate { get; set; } = string.Empty;
    public List<SepidRadisanServiceInsureDto> ServiceInsures { get; set; } = [];
}
