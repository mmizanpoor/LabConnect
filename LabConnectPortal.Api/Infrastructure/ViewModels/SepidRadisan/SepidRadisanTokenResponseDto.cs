using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanTokenResponseDto
{
    [JsonProperty("token")]
    public string? Token { get; set; }

    [JsonProperty("expireDateTime")]
    public DateTime? ExpireDateTime { get; set; }
}
