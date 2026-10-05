using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Rasa;

public class RasaValidationDinaResponseDto
{
    [JsonProperty("result")]
    public RasaValidationResultDto? Result { get; set; }

    [JsonProperty("messages")]
    public List<RasaValidationMessageDto>? Messages { get; set; }
}

public class RasaValidationResultDto
{
    [JsonProperty("claim")]
    public RasaValidationClaimDto? Claim { get; set; }

    [JsonProperty("claimServiceItemList")]
    public List<RasaValidationClaimServiceItemDto>? ClaimServiceItemList { get; set; }

    [JsonProperty("serviceGroupResponseDto")]
    public string? ServiceGroupResponseDto { get; set; }
}

public class RasaValidationMessageDto
{
    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("code")]
    public string? Code { get; set; }
}

public class RasaValidationClaimServiceItemDto
{
    [JsonProperty("itemCode")]
    public string? ItemCode { get; set; }

    [JsonProperty("itemName")]
    public string? ItemName { get; set; }

    [JsonProperty("itemCount")]
    public double ItemCount { get; set; }

    [JsonProperty("itemTotalAmount")]
    public decimal? ItemTotalAmount { get; set; }

    [JsonProperty("itemInsuredAmount")]
    public decimal? ItemInsuredAmount { get; set; }

    [JsonProperty("itemCoveredAmount")]
    public decimal? ItemCoveredAmount { get; set; }

    [JsonProperty("itemFirstInsurerPayAmount")]
    public decimal? ItemFirstInsurerPayAmount { get; set; }

    [JsonProperty("validationStatus")]
    public string? ValidationStatus { get; set; }

    [JsonProperty("validationMessage")]
    public string? ValidationMessage { get; set; }
}

public class RasaValidationClaimDto
{
    [JsonProperty("hcpCompanySiamCode")]
    public string? HcpCompanySiamCode { get; set; }

    [JsonProperty("firstInsurerCompanyCode")]
    public string? FirstInsurerCompanyCode { get; set; }

    [JsonProperty("insurerCompanyCode")]
    public string? InsurerCompanyCode { get; set; }

    [JsonProperty("nationalCode")]
    public string? NationalCode { get; set; }

    [JsonProperty("prescriptionDate")]
    public string? PrescriptionDate { get; set; }

    [JsonProperty("printCode")]
    public string? PrintCode { get; set; }

    [JsonProperty("trackingCode")]
    public string? TrackingCode { get; set; }

    [JsonProperty("trackingCodeDoctorInfo")]
    public RasaResponseDoctorInfoDto? TrackingCodeDoctorInfo { get; set; }

    [JsonProperty("validationStatus")]
    public string? ValidationStatus { get; set; }

    [JsonProperty("validationMessage")]
    public string? ValidationMessage { get; set; }

    [JsonProperty("doctorInfo")]
    public RasaDoctorInfoDto? DoctorInfo { get; set; }

    [JsonProperty("ceilingRemainedAmount")]
    public string? CeilingRemainedAmount { get; set; }

    [JsonProperty("franchisePercent")]
    public string? FranchisePercent { get; set; }

    [JsonProperty("serviceGroupCode")]
    public string? ServiceGroupCode { get; set; }

    [JsonProperty("saveByServiceItem")]
    public string? SaveByServiceItem { get; set; }
}

public class RasaDoctorInfoDto
{
    [JsonProperty("fullName")]
    public string? FullName { get; set; }

    [JsonProperty("specialityName")]
    public string? SpecialityName { get; set; }

    [JsonProperty("nezamCode")]
    public string? NezamCode { get; set; }
}

public class RasaResponseDoctorInfoDto
{
    [JsonProperty("fullName")]
    public string? FullName { get; set; }

    [JsonProperty("specialityName")]
    public string? SpecialityName { get; set; }

    [JsonProperty("nezamCode")]
    public string? NezamCode { get; set; }
}
