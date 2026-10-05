using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanDischargeRequestDto
{
    [JsonProperty("dischargeDate")]
    public string DischargeDate { get; set; } = string.Empty;
}
