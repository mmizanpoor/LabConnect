using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Rasa;

public class RasaCancelDinaCommand
{
    [JsonProperty("trackingCode")]
    public string? TrackingCode { get; set; }

    [JsonProperty("insurerCompanyCode")]
    public string? InsurerCompanyCode { get; set; }

    [JsonProperty("hcpCompanySiamCode")]
    public string HcpCompanySiamCode { get; set; }
}

public class RasaCancelDinaResponseDto : IRasaDinaResponse
{
    [JsonProperty("errorDetail")]
    public RasaErrorDetailDto? ErrorDetail { get; set; }

    [JsonProperty("validationStatus")]
    public string? ValidationStatus { get; set; }

    [JsonProperty("validationMessage")]
    public string? ValidationMessage { get; set; }
}
