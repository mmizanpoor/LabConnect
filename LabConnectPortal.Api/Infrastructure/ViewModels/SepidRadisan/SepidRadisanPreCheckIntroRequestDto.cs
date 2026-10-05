using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanPreCheckIntroRequestDto
{
    [JsonProperty("policyId")]
    public long PolicyId { get; set; }

    [JsonProperty("insuredId")]
    public long InsuredId { get; set; }

    [JsonProperty("doctorMedicalSystemCode")]
    public string DoctorMedicalSystemCode { get; set; } = string.Empty;

    [JsonProperty("medicalSpecialty")]
    public string MedicalSpecialty { get; set; } = string.Empty;

    [JsonProperty("services")]
    public List<SepidRadisanServiceInsureDto> Services { get; set; } = [];
}
