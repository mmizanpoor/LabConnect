using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class Advertisement
{
    [Key]
    public Guid AdvertisementId { get; set; }

    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ShortDescription { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ImagePath { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public bool IsActive { get; set; } = true;

    public int? AdvertisementPositionId { get; set; }

    public Guid? AdvertisementOrderId { get; set; }

    public int SortOrder { get; set; }

    [MaxLength(500)]
    public string? LinkUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedByUserId { get; set; }

    public virtual AdvertisementPosition? Position { get; set; }

    public virtual AdvertisementOrder? Order { get; set; }

    public virtual User? CreatedByUser { get; set; }
}
