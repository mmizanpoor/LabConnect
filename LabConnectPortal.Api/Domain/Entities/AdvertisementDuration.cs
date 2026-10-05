using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class AdvertisementDuration
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Title { get; set; } = string.Empty;

    public int DaysCount { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<AdvertisementPrice> Prices { get; set; } = new List<AdvertisementPrice>();

    public virtual ICollection<AdvertisementOrder> Orders { get; set; } = new List<AdvertisementOrder>();
}
