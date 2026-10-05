using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanApiErrorResponseDto
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("statusCode")]
    public int StatusCode { get; set; }

    [JsonProperty("message")]
    public string? Message { get; set; }
}
