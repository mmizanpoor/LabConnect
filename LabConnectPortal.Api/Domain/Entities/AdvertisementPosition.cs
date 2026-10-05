using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class AdvertisementPosition
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public int MaxConcurrentSlots { get; set; }

    public int? MaxDisplayCount { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AdvertisementPrice> Prices { get; set; } = new List<AdvertisementPrice>();

    public virtual ICollection<AdvertisementOrder> Orders { get; set; } = new List<AdvertisementOrder>();
}
