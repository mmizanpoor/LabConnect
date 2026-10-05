using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class AdvertisementOrder
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int AdvertisementPositionId { get; set; }

    public int AdvertisementDurationId { get; set; }

    public Guid AdvertisementPriceId { get; set; }

    public decimal Price { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public AdvertisementOrderStatus Status { get; set; } = AdvertisementOrderStatus.Draft;

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual AdvertisementPosition Position { get; set; } = null!;

    public virtual AdvertisementDuration Duration { get; set; } = null!;

    public virtual AdvertisementPrice PriceRecord { get; set; } = null!;

    public virtual Advertisement? Advertisement { get; set; }
}
