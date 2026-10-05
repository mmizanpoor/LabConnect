using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Rasa;

public class RasaGetContractListResponseDto : IRasaDinaResponse
{
    [JsonProperty("errorDetail")]
    public RasaErrorDetailDto? ErrorDetail { get; set; }

    [JsonProperty("responseMessage")]
    public RasaResponseMessageDto? ResponseMessage { get; set; }

    [JsonProperty("contractInfoList")]
    public List<RasaContractInfoDto>? ContractInfoList { get; set; }
}

public class RasaResponseMessageDto
{
    [JsonProperty("code")]
    public string? Code { get; set; }

    [JsonProperty("errorMessage")]
    public string? ErrorMessage { get; set; }
}

public class RasaContractInfoDto
{
    [JsonProperty("contractName")]
    public string? ContractName { get; set; }

    [JsonProperty("contractNumber")]
    public string? ContractNumber { get; set; }

    [JsonProperty("insurerCompanyName")]
    public string? InsurerCompanyName { get; set; }

    [JsonProperty("insurerCompanyCode")]
    public string? InsurerCompanyCode { get; set; }

    [JsonProperty("regionUnitCode")]
    public string? RegionUnitCode { get; set; }

    [JsonProperty("regionUnitName")]
    public string? RegionUnitName { get; set; }

    [JsonProperty("insurerInfo")]
    public RasaInsurerInfoDto? InsurerInfo { get; set; }

    [JsonProperty("mainInsurerInfo")]
    public RasaInsurerInfoDto? MainInsurerInfo { get; set; }
}

public class RasaInsurerInfoDto
{
    [JsonProperty("nationalCode")]
    public string? NationalCode { get; set; }

    [JsonProperty("fullName")]
    public string? FullName { get; set; }

    [JsonProperty("mobileNumber")]
    public string? MobileNumber { get; set; }

    [JsonProperty("relationTypeName")]
    public string? RelationTypeName { get; set; }

    [JsonProperty("relationTypeCode")]
    public string? RelationTypeCode { get; set; }

    [JsonProperty("birthDate")]
    public string? BirthDate { get; set; }

    [JsonProperty("genderCode")]
    public string? GenderCode { get; set; }
}

public class ResponseRasa
{
    [JsonProperty("result")]
    public string? Result { get; set; }

    [JsonProperty("messages")]
    public List<RasaResponseMessage>? Messages { get; set; }
}

public class RasaResponseMessage
{
    [JsonProperty("description")]
    public string? Description { get; set; }
    [JsonProperty("status")]
    public string? Status { get; set; }
    [JsonProperty("code")]
    public string? Code { get; set; }
}