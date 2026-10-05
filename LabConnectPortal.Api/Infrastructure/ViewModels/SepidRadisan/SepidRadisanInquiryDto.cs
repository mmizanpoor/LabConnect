using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanInquiryDto
{
    [JsonProperty("insuredId")]
    public decimal InsuredId { get; set; }

    [JsonProperty("policyId")]
    public decimal PolicyId { get; set; }
}
