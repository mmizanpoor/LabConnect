using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class AdvertisementPrice
{
    [Key]
    public Guid Id { get; set; }

    public int AdvertisementPositionId { get; set; }

    public int AdvertisementDurationId { get; set; }

    public decimal Price { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedByUserId { get; set; }

    public virtual AdvertisementPosition Position { get; set; } = null!;

    public virtual AdvertisementDuration Duration { get; set; } = null!;

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual ICollection<AdvertisementOrder> Orders { get; set; } = new List<AdvertisementOrder>();
}
