using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Rasa;

public class RasaClaimInfoModelCommand
{
    [JsonProperty("doctorMedicalNumber")]
    public string? DoctorMedicalNumber { get; set; }

    [JsonProperty("claimInfo")]
    public RasaClaimInfoCommand? ClaimInfo { get; set; }

    [JsonProperty("claimServiceItemInfoList")]
    public List<RasaClaimServiceItemInfoCommand>? ClaimServiceItemInfoList { get; set; }

    [JsonProperty("claimServiceGroupInfo")]
    public RasaServiceGroupInfoCommand? ClaimServiceGroupInfo { get; set; }
}

public class RasaClaimInfoCommand
{
    [JsonProperty("mainClaimTypeCode")]
    public string? MainClaimTypeCode { get; set; } = "OUTPATIENT";

    [JsonProperty("hcpCompanySiamCode")]
    public string? HcpCompanySiamCode { get; set; }

    [JsonProperty("firstInsurerCompanyCode")]
    public string? FirstInsurerCompanyCode { get; set; }

    [JsonProperty("insurerCompanyCode")]
    public string? InsurerCompanyCode { get; set; }

    [JsonProperty("nationalCode")]
    public string? NationalCode { get; set; }

    [JsonProperty("printCode")]
    public string? PrintCode { get; set; }

    [JsonProperty("prescriptionDate")]
    public string? PrescriptionDate { get; set; }

    [JsonProperty("eprscTypeCode")]
    public string? EprscTypeCode { get; set; }

    [JsonProperty("inquiryEPRS")]
    public bool InquiryEPRS { get; set; }

    [JsonProperty("doSave")]
    public bool DoSave { get; set; }

    [JsonProperty("saveByServiceItem")]
    public bool SaveByServiceItem { get; set; }

    [JsonProperty("serviceGroupCode")]
    public string ServiceGroupCode { get; set; }
}

public class RasaClaimServiceItemInfoCommand
{
    [JsonProperty("itemCode")]
    public string? ItemCode { get; set; }

    [JsonProperty("itemName")]
    public string? ItemName { get; set; }

    [JsonProperty("itemCount")]
    public int ItemCount { get; set; }

    [JsonProperty("itemTotalAmount")]
    public int ItemTotalAmount { get; set; }

    [JsonProperty("itemFirstInsurerPayAmount")]
    public int ItemFirstInsurerPayAmount { get; set; }
}

public class RasaServiceGroupInfoCommand
{
    [JsonProperty("serviceGroupName")]
    public string? ServiceGroupName { get; set; }

    [JsonProperty("itemsTotalAmount")]
    public long ItemsTotalAmount { get; set; }

    [JsonProperty("itemsFirstInsurerPayAmount")]
    public long ItemsFirstInsurerPayAmount { get; set; }

    [JsonProperty("firstInsurerExtraObligationAmount")]
    public long FirstInsurerExtraObligationAmount { get; set; }

    [JsonProperty("nonInsuredExpense")]
    public long NonInsuredExpense { get; set; }
}
