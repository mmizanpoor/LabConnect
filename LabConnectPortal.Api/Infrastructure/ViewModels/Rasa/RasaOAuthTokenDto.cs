using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Rasa;

public class RasaOAuthTokenDto
{
    [JsonProperty("access_token")]
    public string? AccessToken { get; set; }

    [JsonProperty("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonProperty("expires_in")]
    public int? ExpiresIn { get; set; }
}
