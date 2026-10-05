using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanTokenRequestDto
{
    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;

    [JsonProperty("password")]
    public string Password { get; set; } = string.Empty;
}
