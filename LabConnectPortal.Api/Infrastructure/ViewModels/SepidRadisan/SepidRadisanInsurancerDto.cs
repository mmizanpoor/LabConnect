using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanInsurancerDto
{
    [JsonProperty("insurancerID")]
    public string? InsurancerId { get; set; }

    [JsonProperty("insurancerNationalID")]
    public string? InsurancerNationalId { get; set; }

    [JsonProperty("insurancerCode")]
    public string? InsurancerCode { get; set; }

    [JsonProperty("insurancerName")]
    public string? InsurancerName { get; set; }
}
