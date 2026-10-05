using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanPreCheckIntroResponseDto
{
    [JsonProperty("checksum")]
    public string? Checksum { get; set; }

    [JsonProperty("wageAmount")]
    public int WageAmount { get; set; }

    [JsonProperty("responseServiceItemDTO")]
    public List<SepidRadisanResponseServiceItemDto>? ResponseServiceItemDto { get; set; }
}
