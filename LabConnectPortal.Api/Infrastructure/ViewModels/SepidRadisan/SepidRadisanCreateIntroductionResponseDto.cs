using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanCreateIntroductionResponseDto
{
    [JsonProperty("introductionId")]
    public double IntroductionId { get; set; }

    [JsonProperty("doctorName")]
    public string? DoctorName { get; set; }

    [JsonProperty("doctorMedicalSystemCode")]
    public string? DoctorMedicalSystemCode { get; set; }

    [JsonProperty("medicalSpecialty")]
    public string? MedicalSpecialty { get; set; }

    [JsonProperty("policyId")]
    public decimal PolicyId { get; set; }

    [JsonProperty("services")]
    public List<SepidRadisanResponseServiceItemDto>? Services { get; set; }
}
