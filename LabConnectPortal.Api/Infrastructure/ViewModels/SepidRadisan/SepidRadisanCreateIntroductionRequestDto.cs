using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanCreateIntroductionRequestDto
{
    [JsonProperty("checksum")]
    public string Checksum { get; set; } = string.Empty;

    [JsonProperty("doctorName")]
    public string DoctorName { get; set; } = string.Empty;

    [JsonProperty("insuredId")]
    public long InsuredId { get; set; }

    [JsonProperty("policyId")]
    public long PolicyId { get; set; }

    [JsonProperty("doctorMedicalSystemCode")]
    public string DoctorMedicalSystemCode { get; set; } = string.Empty;

    [JsonProperty("receptionDate")]
    public string ReceptionDate { get; set; } = string.Empty;

    [JsonProperty("doctorOrderDate")]
    public string DoctorOrderDate { get; set; } = string.Empty;

    [JsonProperty("medicalSpecialty")]
    public string MedicalSpecialty { get; set; } = string.Empty;

    [JsonProperty("discharged")]
    public bool Discharged { get; set; }

    [JsonProperty("services")]
    public List<SepidRadisanServiceInsureDto> Services { get; set; } = [];
}
