using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

public class SepidRadisanInsuredInfoDto
{
    [JsonProperty("insuredId")]
    public string? InsuredId { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("family")]
    public string? Family { get; set; }

    [JsonProperty("fatherName")]
    public string? FatherName { get; set; }

    [JsonProperty("birthDate")]
    public string? BirthDate { get; set; }

    [JsonProperty("nationalCode")]
    public string? NationalCode { get; set; }
}
