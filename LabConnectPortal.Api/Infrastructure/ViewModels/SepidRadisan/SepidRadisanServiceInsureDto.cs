using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanServiceInsureDto
{
    [JsonProperty("serviceCount")]
    public int? ServiceCount { get; set; }

    [JsonProperty("serviceName")]
    public string? ServiceName { get; set; }

    [JsonProperty("serviceCode")]
    public int? ServiceCode { get; set; }

    [JsonProperty("serviceAmount")]
    public int? ServiceAmount { get; set; }

    [JsonProperty("baseInsurerPortionAmount")]
    public int? BaseInsurerPortionAmount { get; set; }
}
