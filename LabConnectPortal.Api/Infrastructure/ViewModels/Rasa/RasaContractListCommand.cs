using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Rasa;

public class RasaContractListCommand
{
    [JsonProperty("nationalCode")]
    public string? NationalCode { get; set; }

    [JsonProperty("insurerCompanyCode")]
    public string? InsurerCompanyCode { get; set; }

    [JsonProperty("hcpCompanySiamCode")]
    public string? HcpCompanySiamCode { get; set; }
}
