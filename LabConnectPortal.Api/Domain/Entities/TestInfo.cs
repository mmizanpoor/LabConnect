using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class TestInfo
{
    public long Id { get; set; }

    public int LabCode { get; set; }

    public int LabCodeNew { get; set; }

    [MaxLength(50)]
    public string? CPNCode { get; set; }

    [MaxLength(50)]
    public string? NationalCode { get; set; }

    [MaxLength(1000)]
    public string? MeasurName { get; set; }

    [MaxLength(1000)]
    public string? FullName { get; set; }

    [MaxLength(1000)]
    public string? ShortName { get; set; }

    [MaxLength(1000)]
    public string? SectionName { get; set; }

    [MaxLength(1000)]
    public string? SimilarName { get; set; }

    [MaxLength(1000)]
    public string? KD { get; set; }

    [MaxLength(1000)]
    public string? Volume { get; set; }

    [MaxLength(1000)]
    public string? MinVolume { get; set; }

    [MaxLength(1000)]
    public string? Maintenance { get; set; }

    [MaxLength(1000)]
    public string? Transportation { get; set; }

    [MaxLength(1000)]
    public string? Needs { get; set; }

    [MaxLength(1000)]
    public string? Guidance { get; set; }

    [MaxLength(1000)]
    public string? PatientInfo { get; set; }

    [MaxLength(1000)]
    public string? Denial { get; set; }

    [MaxLength(1000)]
    public string? Preparation { get; set; }

    [MaxLength(1000)]
    public string? ClinicalInfo { get; set; }

    [MaxLength(1000)]
    public string? Sources { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    [MaxLength(1000)]
    public string? Caution { get; set; }

    [MaxLength(1000)]
    public string? SClinical { get; set; }

    [MaxLength(1000)]
    public string? Detail { get; set; }

    [MaxLength(1000)]
    public string? Date { get; set; }

    [MaxLength(1000)]
    public string? ResultDuration { get; set; }

    [MaxLength(1000)]
    public string? MaxDurResult { get; set; }

    [MaxLength(1000)]
    public string? MaintenanceDur { get; set; }

    public long TestId { get; set; }

    [MaxLength(1000)]
    public string? Criteria { get; set; }

    [MaxLength(1000)]
    public string? Freezer { get; set; }

    [MaxLength(1000)]
    public string? DeliveryCondition { get; set; }

    public decimal? ApprovePrice { get; set; }

    public long? KitGroupId { get; set; }

    public long? DeviceGroupId { get; set; }

    public KitGroup? KitGroup { get; set; }

    public DeviceGroup? DeviceGroup { get; set; }
}
