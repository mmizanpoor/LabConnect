using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanPolicyInfoDto
{
    [JsonProperty("policyId")]
    public decimal PolicyId { get; set; }

    [JsonProperty("policyFullNo")]
    public string? PolicyFullNo { get; set; }

    [JsonProperty("policyHolderName")]
    public string? PolicyHolderName { get; set; }

    [JsonProperty("policyStartDate")]
    public string? PolicyStartDate { get; set; }

    [JsonProperty("policyEndDate")]
    public string? PolicyEndDate { get; set; }
}
