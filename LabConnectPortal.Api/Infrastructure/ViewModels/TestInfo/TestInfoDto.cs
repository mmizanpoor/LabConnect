namespace LabConnectPortal.Api.Infrastructure.ViewModels.TestInfo;

public class TestInfoListItemDto
{
    public long Id { get; set; }

    public string? CPNCode { get; set; }

    public string? NationalCode { get; set; }

    public string? FullName { get; set; }

    public string? ShortName { get; set; }

    public string? SectionName { get; set; }

    public decimal? ApprovePrice { get; set; }
}

public class TestInfoDetailDto
{
    public long Id { get; set; }

    public int LabCode { get; set; }

    public int LabCodeNew { get; set; }

    public string? CPNCode { get; set; }

    public string? NationalCode { get; set; }

    public string? MeasurName { get; set; }

    public string? FullName { get; set; }

    public string? ShortName { get; set; }

    public string? SectionName { get; set; }

    public string? SimilarName { get; set; }

    public string? KD { get; set; }

    public string? Volume { get; set; }

    public string? MinVolume { get; set; }

    public string? Maintenance { get; set; }

    public string? Transportation { get; set; }

    public string? Needs { get; set; }

    public string? Guidance { get; set; }

    public string? PatientInfo { get; set; }

    public string? Denial { get; set; }

    public string? Preparation { get; set; }

    public string? ClinicalInfo { get; set; }

    public string? Sources { get; set; }

    public string? Comment { get; set; }

    public string? Caution { get; set; }

    public string? SClinical { get; set; }

    public string? Detail { get; set; }

    public string? Date { get; set; }

    public string? ResultDuration { get; set; }

    public string? MaxDurResult { get; set; }

    public string? MaintenanceDur { get; set; }

    public long TestId { get; set; }

    public string? Criteria { get; set; }

    public string? Freezer { get; set; }

    public string? DeliveryCondition { get; set; }

    public decimal? ApprovePrice { get; set; }

    public long? KitGroupId { get; set; }

    public string? KitGroupTitle { get; set; }

    public long? DeviceGroupId { get; set; }

    public string? DeviceGroupTitle { get; set; }
}

public class UpdateTestInfoCommand
{
    public long Id { get; set; }

    public decimal? ApprovePrice { get; set; }

    public long? KitGroupId { get; set; }

    public long? DeviceGroupId { get; set; }
}

public class UpdateTestInfoApprovePriceCommand
{
    public long Id { get; set; }

    public decimal? ApprovePrice { get; set; }
}
