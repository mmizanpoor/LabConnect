using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanResponseServiceItemDto
{
    [JsonProperty("categoryCode")]
    public string? CategoryCode { get; set; }

    [JsonProperty("serviceCode")]
    public string? ServiceCode { get; set; }

    [JsonProperty("serviceCount")]
    public string? ServiceCount { get; set; }

    [JsonProperty("payableAmount")]
    public string? PayableAmount { get; set; }

    [JsonProperty("baseInsurerPortionAmount")]
    public string? BaseInsurerPortionAmount { get; set; }

    [JsonProperty("message")]
    public string? Message { get; set; }

    [JsonProperty("toothNum")]
    public string? ToothNum { get; set; }
}
