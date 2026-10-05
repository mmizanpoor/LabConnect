using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Rasa;

public class RasaErrorDetailDto
{
    [JsonProperty("errorCode")]
    public string? ErrorCode { get; set; }

    [JsonProperty("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonProperty("stackTrace")]
    public string? StackTrace { get; set; }
}
