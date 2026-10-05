using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanDeleteIntroductionRequestDto
{
    [JsonProperty("IntroductionId")]
    public string IntroductionId { get; set; } = string.Empty;
}
